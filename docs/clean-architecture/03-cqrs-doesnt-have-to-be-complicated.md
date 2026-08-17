# CQRS Doesn't Have To Be Complicated | Clean Architecture, .NET 6

- **Video:** https://youtube.com/watch?v=vdi-p9StmG0
- **Duration:** 24:09
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 3/22

## Key Learnings

- You do **not** need separate read/write databases, event sourcing, or other heavy machinery to "do CQRS" — a single database with logically separated command/query code paths is a perfectly valid starting point. Add complexity (separate DBs, projections, event sourcing) only when you actually need to scale.
- Full from-scratch implementation of custom CQRS abstractions on top of Mediator (rather than using Mediator's raw `IRequest`/`IRequestHandler` directly), applied to a "Members" feature (create + get-by-id).

## Concepts & Patterns

- Write side: commands go through the **rich domain model** (domain entities/aggregates). Read side: queries project directly to API-friendly response shapes, decoupled from the write model.
- Naming convention: `<Action><Entity>Command`/`Query` + `<Same Name>Handler`. One class per file (handler kept separate from the command/query definition, not nested inside it) — also makes it possible to split commands/handlers into different assemblies if desired.

## Code/Structure Notes

Custom abstractions (in `Abstractions/Messaging`) wrap Mediator's interfaces so all use cases uniformly return a `Result`:

```csharp
public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

- Command example: `CreateMemberCommand(Email, FirstName, LastName) : ICommand<Guid>`; handler creates the member, adds via repository, calls `SaveChangesAsync` on unit of work, returns `Result.Success(id)`.
- Query example: `GetMemberByIdQuery(MemberId) : IQuery<MemberResponse>`; handler fetches via repository (`GetByIdAsync` returns nullable), returns `Result.Failure<MemberResponse>(Error)` when null, otherwise wraps a `MemberResponse` DTO in `Result.Success`.
- Controllers derive from a shared `ApiController` base exposing a `protected readonly ISender _sender` (use `ISender` instead of full `IMediator` if you only send commands/queries, not publish notifications).
- DI wiring: `AddMediator(cfg)` pointed at the assembly containing handlers (via an `AssemblyReference` marker class); `AddControllers()` + `.AddApplicationPart(presentationAssembly)`; Scrutor scans Infrastructure/Persistence assemblies to auto-register implementations against their interfaces (scoped lifetime).

## Implementation Steps

1. Define `ICommand`/`ICommand<T>`/`ICommandHandler`/`IQuery<T>`/`IQueryHandler` wrapping Mediator's `IRequest`/`IRequestHandler`, all returning `Result`/`Result<T>`.
2. Implement command + handler for writes (repository + unit-of-work pattern), query + handler for reads (repository or raw query, projecting into a response DTO).
3. Register Mediator against the assembly holding your handlers; register controllers via `AddApplicationPart`.
4. In controller actions: build the command/query, `await _sender.Send(x, cancellationToken)`, branch on `result.IsSuccess` → `Ok(...)` vs `BadRequest(result.Error)` / `NotFound(result.Error)`.

## Gotchas & Tips

- Defining your own `ICommand`/`IQuery` marker interfaces (instead of using Mediator's `IRequest` directly) is more explicit and lets you standardize the `Result` return type across every use case, so callers always know to check `IsSuccess`.
- Always pass `CancellationToken` through from the controller action to `Send(...)`.
- Next video in series covers pipeline behaviors (validation, logging, exception handling).
