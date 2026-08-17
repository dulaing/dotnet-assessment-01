# Why I Use The Unit of Work Pattern With EF Core | Clean Architecture

- **Video:** https://youtube.com/watch?v=vN_j1Bs0ALU
- **Duration:** 11:34
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 20/22

## Key Learnings

- The Unit of Work (UoW) here starts as a thin wrapper around `DbContext.SaveChangesAsync()`, exposed via an `IUnitOfWork` interface that lives in the **Application layer** (not Infrastructure) so the application layer never references EF Core directly — keeps Clean Architecture's dependency direction intact.
- Why wrap `SaveChanges` instead of calling `DbContext` directly from handlers:
  1. Combined with the **repository pattern**, repositories deliberately have **no `SaveChanges`/persist method** — persistence responsibility is centralized solely in `IUnitOfWork.SaveChangesAsync()`, called once at the end of a business transaction (single responsibility).
  2. `IUnitOfWork` is trivially mockable in unit tests for command handlers (vs. mocking `DbContext`).
- UoW acts as a **transaction boundary**: any number of entity changes made during a handler are persisted together in one `SaveChanges` call.

## Concepts & Patterns

- Milan **moves logic out of EF Core `SaveChangesInterceptor`s and into the UoW's `SaveChangesAsync` override**, calling two private helper methods before the base save:
  1. `ConvertDomainEventsToOutboxMessages()` — pulls domain events off tracked entities and converts them into outbox message rows added to the appropriate `DbSet` (Outbox pattern — see video 21 for the domain-events side).
  2. `UpdateAuditableEntities()` — iterates `ChangeTracker` entries implementing an `IAuditableEntity` interface; sets `CreatedOn` when `EntityState.Added`, sets `ModifiedOn` when `EntityState.Modified`.
- Both moved-out interceptors are then **removed from DI registration** in `Program.cs` since their behavior now lives in the UoW.

```csharp
public async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    ConvertDomainEventsToOutboxMessages();
    UpdateAuditableEntities();
    return await _dbContext.SaveChangesAsync(ct);
}
```

## Implementation Steps

1. Define `IUnitOfWork` (Application layer) with `SaveChangesAsync`.
2. Implement it in Infrastructure wrapping `DbContext`.
3. Repositories: `Add`/`Update` only mutate the change tracker — no save call.
4. Command handler flow: fetch aggregate via repository → call domain method(s) → `repository.Update(entity)` → `unitOfWork.SaveChangesAsync()` once.
5. Move interceptor logic (domain-events-to-outbox conversion, auditable-entity timestamping) into UoW's `SaveChangesAsync` override; delete the interceptor registrations.

## Gotchas & Tips

- Debugged with a real request (`PUT` updating a member's name via Postman): confirms one `SaveChanges` call produces **two SQL statements in one transaction** — an `UPDATE members` and an `INSERT INTO outbox_messages` — proving domain event → outbox conversion and the entity update are atomic.
- Side note: EF Core's SQL Server provider sends *all* mapped columns in an `UPDATE` (not just changed ones), whereas the **PostgreSQL provider only sends changed columns** — one reason Milan prefers Postgres over SQL Server.
