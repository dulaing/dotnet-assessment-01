# Rebuild-by-Hand Learning Plan

A curriculum for rebuilding this Library API from an empty project, one concept
at a time, so you understand every moving part instead of inheriting a finished
app you can't explain.

The finished app already lives in this repo. Treat it as an **answer key**, not
a thing to copy. You look at it only *after* you've tried the piece yourself.

---

## Ground rules (read once)

1. **Answer key = `main`.** The full reference implementation is on the `main`
   branch. Your rebuild happens on a fresh branch. Whenever you're stuck, you
   diff your file against the reference:
   - `git show main:Library.Api/Program.cs` prints the reference version.
   - `git difftool main -- Library.Api/Program.cs` compares yours to it.
   - Rule: **write it yourself first, peek second.** Peeking before trying is
     how you end up back where you started.

2. **Type every line.** No copy-paste from the answer key. Muscle memory and
   compiler errors are the point.

3. **One concept per sitting.** Each phase below is a self-contained lesson with
   a *goal*, the *concepts* it teaches, what to *build*, and a *checkpoint* — a
   question you should be able to answer out loud before moving on. If you can't
   answer the checkpoint, you haven't learned it yet.

4. **Break things on purpose.** Most phases have a "poke it" step. Delete a line,
   see the error, understand *why* it broke. The errors teach more than the
   working code.

5. **Keep a `NOTES.md`.** One file where you write, in your own words, the answer
   to each checkpoint. If you can't write it plainly, you don't get it yet.

### Suggested setup (do this before Phase 1)

```
git checkout main
git checkout -b learn/rebuild-by-hand      # your working branch
# then gut the app back to near-empty so you rebuild it:
#   - delete everything under Library.Api EXCEPT the .csproj and Properties/
#   - delete Migrations/, delete the tests, keep docker-compose.yml
```

Don't delete `docs/` — this plan lives there. Everything you delete is safe on
`main` forever.

---

## Phase 0 — The empty skeleton

**Goal.** A Web API that boots and returns nothing useful. Understand what a
"project" even is before adding features.

**Concepts.**
- What `dotnet new`, `.csproj`, `dotnet run`, `dotnet build` actually do.
- The SDK vs the runtime. `<TargetFramework>net10.0</TargetFramework>`.
- What `bin/` and `obj/` are (and why they're gitignored).

**Build.**
- `dotnet new web -n Library.Api` (or reuse the existing empty `.csproj`).
- Get it to `dotnet run` and respond to `GET /` with `"hello"`.

**Poke it.** Delete `app.Run()`. Read the error. Put it back.

**Checkpoint.** What does `dotnet run` do that `dotnet build` doesn't? Where does
the compiled output go?

---

## Phase 1 — Program.cs, the builder, and configuration layering

This is the phase you asked about most. Go slow here.

**Goal.** Understand the `builder -> build -> run` lifecycle and *exactly* how
configuration values get chosen at runtime.

**Concepts.**
- `WebApplication.CreateBuilder(args)` — what it wires up for you (config,
  logging, DI container) before you touch anything.
- The **configuration layering order**: `appsettings.json` <- 
  `appsettings.{Environment}.json` <- environment variables <- command-line.
  Later layers *override* earlier ones. This is why the connection string in
  `appsettings.Development.json` wins over `appsettings.json` when you run in Dev.
- **How the environment is chosen.** `ASPNETCORE_ENVIRONMENT` is set in
  `Properties/launchSettings.json` (look at the `http`/`https` profiles). That
  env var is what makes `builder.Environment.IsDevelopment()` true, which is
  what makes the `.Development.json` file load *and* Swagger turn on.
- `builder.Configuration.GetConnectionString("X")` is just sugar for
  `Configuration["ConnectionStrings:X"]`.
- CORS: `builder.Services.AddCors(...)` + `app.UseCors(...)`, reading allowed
  origins from config so they differ per environment.

**Build.**
- Two `appsettings` files with a **different** value for the same key, so you can
  watch which one wins.
- Read the connection string in `Program.cs`, `throw` if missing (see the
  reference lines 15-16 for the pattern — after you've tried).
- Add a CORS policy whose allowed origins come from config, not hardcoded.
- Log `app.Environment.EnvironmentName` at startup so you can *see* it.

**Poke it.**
- Run with the `http` profile, then run `ASPNETCORE_ENVIRONMENT=Production dotnet run`
  and watch the config value flip and Swagger disappear.
- Delete the connection string from config and watch your `throw` fire.

**Checkpoint.** Walk the chain out loud: launchSettings sets an env var ->
that selects the environment -> that loads a specific appsettings file -> that
supplies the connection string. Where exactly in that chain would you change the
DB for production only?

---

## Phase 2 — Postgres, Npgsql, and the DbContext (no repository yet)

**Goal.** Talk to a real database. On purpose, hit the `DbContext` *directly*
from an endpoint so you feel why layering comes later.

**Concepts.**
- Docker Compose brings up Postgres (`docker compose up -d`). What the env vars
  in `docker-compose.yml` map to (db name, user, password) and how they line up
  with your connection string.
- **Npgsql** = the PostgreSQL ADO.NET driver. `UseNpgsql(connectionString)` tells
  EF Core to speak the Postgres wire protocol and translate LINQ into Postgres SQL.
- `DbContext` = your session with the DB + a `DbSet<T>` per table.
- Registering it: `AddDbContext<LibraryDbContext>` and *why* it's scoped
  (one context per request).

**Build.**
- One entity, `Book`, plain properties only (no behavior yet).
- A `LibraryDbContext` with `DbSet<Book> Books`.
- Register it in `Program.cs`.
- Temporarily map `GET /api/books` that injects the context and returns
  `await db.Books.ToListAsync()`. Yes, this is the "wrong" architecture. You'll
  fix it in Phase 8 and appreciate why.

**Poke it.** Turn on SQL logging (`options.UseNpgsql(...).LogTo(Console.WriteLine)`)
and watch the actual SQL EF sends. This demystifies the whole ORM.

**Checkpoint.** What does Npgsql do that EF Core doesn't, and vice versa? Why is
the DbContext registered as *scoped* and not singleton?

---

## Phase 3 — Migrations, the snapshot, and the history table

**Goal.** Understand how EF turns C# classes into DB tables *and* how it knows
what's already applied.

**Concepts.**
- `dotnet ef migrations add InitialCreate` generates **three** things: the
  migration `.cs` (Up/Down), the migration `.Designer.cs`, and it updates
  `LibraryDbContextModelSnapshot.cs`.
- **Up / Down.** `Up()` moves the schema forward; `Down()` reverses it. `dotnet ef
  database update` runs pending `Up()`s; `database update <PrevMigration>` runs
  `Down()`s.
- **The snapshot is EF's memory.** When you add a *new* migration, EF diffs your
  current model against the snapshot to compute the delta. That's how it knows
  which changes are already captured — it's comparing to the snapshot, not the DB.
- **The Designer file** pins the model *as it was at that migration*, so old
  migrations stay reproducible even after the model evolves.
- **`__EFMigrationsHistory`** is a table EF creates in Postgres. Each applied
  migration's ID gets a row. That's how EF knows, at `database update` time,
  which migrations the *database* has already seen.

**Build.**
- `dotnet ef migrations add InitialCreate`, then `dotnet ef database update`.
- Open all three generated files and read them line by line.
- Connect to Postgres (`docker exec ... psql`) and `SELECT * FROM "__EFMigrationsHistory";`.

**Poke it.**
- Add a property to `Book`, run `migrations add AddSomething`, and read the new
  `Up()` — notice it only contains the *delta*, because of the snapshot diff.
- Run `database update <InitialCreate>` to roll back and watch the `Down()` run.

**Checkpoint.** You add a column, then delete the snapshot file and try to add
another migration. What goes wrong, and why? (Explains the snapshot's whole job.)

---

## Phase 4 — Relationships, foreign keys, and navigation properties

This is the part that isn't even in the reference yet, so you're genuinely
building, not copying.

**Goal.** Model `Book`, `Member`, `Borrowing` and understand FK conventions and
navigation properties from the ground up.

**Concepts.**
- **FK by convention.** A `public int BookId` on `Borrowing` + a `public Book
  Book` navigation property makes EF infer a FK relationship with zero config.
- **Navigation properties** are the object-graph view (`borrowing.Book.Title`)
  over what is, in the DB, just an integer FK column.
- One-to-many: `Book` has `public List<Borrowing> Borrowings`, `Borrowing` has
  `public Book Book`. EF wires both ends.
- **Two ways to configure the same relationship:** convention/navigation vs the
  explicit `HasOne<Book>().WithMany().HasForeignKey(...)` the reference uses
  (which has *no* nav property — a deliberately leaner model). Build it both ways
  and compare the generated migration. They produce the same table.
- `Include(b => b.Borrowings)` produces a SQL **JOIN**. Look at the SQL.

**Build.**
- All three entities with navigation properties *this* time (the richer version).
- A migration for the new tables + FKs; read the `Up()` to see `AddForeignKey`.
- An endpoint using `.Include(...)` with SQL logging on, so you *see* the join.

**Checkpoint.** Where does the FK actually live — on the entity, or only in the
DB? What's the difference between the `BookId` property and the `Book` nav
property at runtime when the nav isn't loaded?

---

## Phase 5 — The three loading strategies

**Goal.** Feel the difference between eager, explicit, and lazy loading and know
when each bites you.

**Concepts.**
- **Eager:** `.Include(b => b.Borrowings)` — load related data in the same query
  (JOIN). Predictable, one round trip.
- **Explicit:** `db.Entry(book).Collection(b => b.Borrowings).LoadAsync()` — load
  on demand, by hand, later.
- **Lazy:** access `book.Borrowings` and EF silently fires a query. Requires
  `UseLazyLoadingProxies()` + `virtual` nav properties. Convenient, and the
  classic source of the **N+1 query problem**.

**Build.** A throwaway `LoadingExperiments.cs` (a console-style endpoint or a
test) that does the *same* read three ways, SQL logging on. Count the queries
each strategy fires over a list of books each with borrowings. Watch lazy
loading explode into N+1.

**Checkpoint.** You're returning 100 books each with their borrowings. Which
strategy, and why? When is explicit loading actually the right call?

---

## Phase 6 — The change tracker

**Goal.** Understand what `SaveChanges` actually does and why you rarely call
`Update()` explicitly.

**Concepts.**
- The **ChangeTracker** watches every entity the context loaded and records its
  `EntityState`: `Unchanged`, `Added`, `Modified`, `Deleted`, `Detached`.
- You load a `Book`, change `b.Title`, call `SaveChanges()` — EF emits an
  `UPDATE` for *only the changed column* because the tracker diffed it. You never
  called `Update`.
- `AsNoTracking()` for read-only queries: faster, no tracking overhead, but then
  edits won't be detected.
- This is also *why* the reference stamps audit fields inside `SaveChanges` by
  looping `ChangeTracker.Entries<IAuditable>()` (see `LibraryDbContext` on
  `main`) — it inspects states right before saving.

**Build.** A `ChangeTrackingExperiments.cs` that loads an entity, prints
`db.Entry(x).State`, mutates a field, prints the state again (watch it flip to
`Modified`), saves, prints once more (back to `Unchanged`). Do the same with
`AsNoTracking()` and watch the mutation *not* persist.

**Checkpoint.** You loaded a book with `AsNoTracking()`, changed the title, and
called `SaveChanges()`. Nothing happened. Why? How would you force the update?

---

## Phase 7 — Contracts (DTOs) and validation

**Goal.** Stop exposing entities. Validate input properly.

**Concepts.**
- **DTOs vs entities.** Request/response records in `Contracts/` decouple your API
  shape from your DB shape. Never return an EF entity from an endpoint.
- Validation, layered from simple to real:
  1. Manual `if` checks (feel the pain).
  2. Data annotations (`[Required]`, `[EmailAddress]`).
  3. **FluentValidation** — a validator class per request, registered via
     `AddValidatorsFromAssemblyContaining<>`, run by an endpoint filter so
     handlers never see invalid input. This is the reference approach.
- Mapping DTO <-> entity by hand (understand it before reaching for AutoMapper).
- The consistent error payload shape (`statusCode`, `message`, `errors[]`).

**Build.**
- `CreateBookRequest`, `BookResponse`, etc. as `record`s.
- Start with manual validation in the handler. Then rewrite one as a
  FluentValidation validator + a validation endpoint filter. Compare how much
  cleaner the handler gets.

**Checkpoint.** Why not just add `[Required]` to the entity and skip DTOs? Give
two concrete problems that causes.

---

## Phase 8 — Repository pattern and Unit of Work

Now you fix the "endpoint hits DbContext" sin from Phase 2, and you'll actually
feel *why*.

**Goal.** Put a seam between business logic and EF.

**Concepts.**
- **Repository** = a collection-like abstraction over a table
  (`IBookRepository.GetByIdAsync`). Endpoints/services depend on the interface,
  not on `DbContext`.
- **Unit of Work.** Here's the honest part: **EF Core's `DbContext` already *is*
  a Unit of Work** (it batches changes and commits them in one `SaveChanges`
  transaction). So a separate `IUnitOfWork` is often redundant for a simple API.
- **Why learn it anyway** (your stated reason — the long game): in larger apps
  you want one explicit `SaveChanges` boundary coordinating *multiple*
  repositories in one transaction, and you want to hide EF entirely from the
  application layer. Build a thin `IUnitOfWork` wrapping `SaveChangesAsync` so you
  understand the pattern, even while knowing it's optional here.
- The reference's repositories (on `main`) share the context and let the *service*
  call `SaveChanges` — read them and notice there's no separate UoW class,
  because the context fills that role. Understand that tradeoff.

**Build.**
- `IBookRepository` + `BookRepository`, register scoped, refactor the Phase 2
  endpoint to go through it.
- Then add an explicit `IUnitOfWork` wrapping the context's save, wire one
  service through it, and write a paragraph in `NOTES.md` on whether it earned
  its keep here.

**Checkpoint.** Someone says "the repository pattern is pointless with EF because
the DbContext is already a repository + unit of work." Steelman them, then give
the counter-argument for an enterprise codebase.

---

## Phase 9 — Application services and domain behavior

**Goal.** Move business rules out of endpoints and into services + the domain.

**Concepts.**
- **Thin endpoints.** The target flow: `Endpoint -> Service -> Repository ->
  DbContext`. The endpoint parses the request, calls a service, maps the result
  to a response. Nothing else.
- **Rich domain behavior.** Rules that protect an entity's invariants live *on*
  the entity: `book.BorrowCopy()`, `borrowing.ReturnBook()` (see `Book.cs` /
  `Borrowing.cs` on `main`). Rules that span entities or need data access live in
  the service (e.g. "a member can't have more than 3 active borrowings").
- Custom exceptions (`NotFoundException`, `ConflictException`) as the vocabulary
  services throw, translated to HTTP later.

**Build.**
- `BookService`, `MemberService`, `BorrowingService` behind interfaces.
- Put copy-count logic on the entities; put cross-entity rules in the service.
- Implement the full borrowing flow: check member active, check availability,
  check the 3-book limit, decrement copies, set the 14-day due date.

**Checkpoint.** Which borrowing rules belong *on* the entity and which belong in
the service? State the principle you used to decide, not just the list.

---

## Phase 10 — Middleware and global error handling

**Goal.** Understand the request pipeline and centralize error-to-HTTP mapping.

**Concepts.**
- **Middleware = a pipeline.** Each `app.Use...` wraps the next. Order matters.
  `UseExceptionHandler` near the top catches everything below it.
- `IExceptionHandler` + `AddProblemDetails` (the reference's `GlobalExceptionHandler`
  on `main`) turns your `NotFoundException` -> 404, `ConflictException` -> 409,
  validation failures -> 400, each with the consistent payload and a `traceId`.
- Why this beats try/catch in every handler.

**Build.**
- A `GlobalExceptionHandler : IExceptionHandler` mapping your custom exceptions to
  status codes + the standard error body.
- Register it, remove any per-handler try/catch, confirm a thrown
  `NotFoundException` surfaces as a clean 404.

**Poke it.** Move `UseExceptionHandler` to *after* your endpoints and watch
errors stop being caught. Order is the lesson.

**Checkpoint.** Trace a `NotFoundException` from `throw` in a service to the JSON
the client receives. Name each pipeline stage it passes through.

---

## Phase 11 — MediatR and the CQRS shape (the enterprise layer)

**Goal.** See what MediatR buys you and, just as important, what it costs.

**Concepts.**
- **MediatR** decouples caller from handler: an endpoint sends a `Command`/`Query`
  object; a matching `Handler` runs it. No direct service reference.
- **CQRS-lite:** commands (writes) and queries (reads) as separate request types
  and handlers. Often reads can then skip the domain and hit EF directly.
- **Pipeline behaviors** (validation, logging, transactions) wrap every handler —
  the clean home for cross-cutting concerns, replacing the endpoint filter.
- **Honest cost:** more files, more indirection, harder to trace for a small app.
  It pays off when handler count and cross-cutting concerns grow.

**Build.** Refactor *one* feature (say, create book) end to end: a `CreateBookCommand`,
its handler, a validation pipeline behavior, and an endpoint that just sends the
command. Leave the rest as services so you can compare the two styles side by side
in the same codebase.

**Checkpoint.** For *this* size of app, is MediatR worth it? Answer with the
specific thing it improved and the specific thing it made worse.

---

## Phase 12 — Tests for the business rules

**Goal.** Lock the rules that matter with fast tests.

**Concepts.**
- Unit-testing services/domain by faking repositories (no DB needed).
- What's worth testing: the borrowing rules, not framework plumbing.
- (Bonus) **Testcontainers** to spin real Postgres for integration tests.

**Build.** Tests for: can't borrow with 0 copies; inactive member can't borrow;
can't exceed 3 active borrowings; return increments copies; can't return twice.
(The reference `Library.Tests` on `main` is your check after you write yours.)

**Checkpoint.** Which of these rules can be tested with zero database, and why?

---

## Phase 13 — Capstone: real Clean Architecture (separate projects)

**Goal.** The current code is Clean Architecture *folders in one project*. Real
Clean Architecture is *separate projects* whose references enforce the dependency
rule at compile time.

**Concepts.**
- Split into projects: `Library.Domain`, `Library.Application`,
  `Library.Infrastructure`, `Library.Api`.
- **The dependency rule, enforced by the compiler:** Domain references nothing.
  Application references Domain. Infrastructure references Application. Api
  references Application + Infrastructure. If Domain tried to `using` EF Core, it
  literally wouldn't compile — that's the point.
- Where each existing folder moves, and which interfaces have to flip (e.g.
  repository *interfaces* live in Application, *implementations* in Infrastructure).

**Build.** Create the four projects, move your code, fix the references until it
compiles with the dependency rule intact. This is the graduation exercise — it
forces you to understand *why* every layer boundary exists.

**Checkpoint.** You want to add a caching decorator around a repository. Which
project does it go in, and why does the dependency rule allow it there but not in
Domain?

---

## How to use the answer key without cheating

For any file, the reference version is one command away:

```
git show main:Library.Api/Application/Services/BorrowingService.cs
```

Workflow per file: try it -> compile -> get it working -> *then* diff against
`main` -> write in `NOTES.md` what the reference did differently and whether it's
better. The diff *after* you've struggled is where the learning compounds.

Good luck. Go slow. The goal isn't a working API — you already have one on
`main`. The goal is being able to explain every line of it.
