# How to Implement the CQRS Pattern in Clean Architecture (from scratch)

- **Video:** https://youtube.com/watch?v=85YbMEb1qkQ
- **Duration:** 17:36
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 4/22

## Key Learnings

- CQRS = splitting write flow (commands) from read flow (queries). Can be logical only (one DB) or physical (separate write/read DBs + projection) — start logical, scale later if needed.
- Traces CQRS's lineage back to **CQS** (Command Query Separation): a method-level principle where commands mutate state and return nothing, queries return data and have no side effects. Milan explicitly breaks the "commands return nothing" rule when it's convenient (e.g. returning a new id).
- This video builds the CQRS abstractions **without Mediator** first, to show what's happening under the hood, then explains why Mediator is still preferred in practice (pipeline behaviors / middleware-style extensibility).

## Concepts & Patterns

- Command side: API request → command → command handler → repository/EF Core hits the rich domain model → domain logic executes → EF Core persists changes. More ceremony, but full control/business-rule enforcement via the domain model and domain services.
- Query side: API request → query → query handler → minimal indirection (e.g. **Dapper** + raw SQL, or EF Core with a LINQ projection straight to a DTO) → fast read, no domain model involved.
- Benefits of splitting: separation of concerns (different requirements for reads vs writes), finer-grained security control over DB access, ability to scale read/write sides independently (most apps are read-heavy).

## Code/Structure Notes

```csharp
public interface IBaseCommand { }
public interface ICommand : IBaseCommand, IRequest<Result> { }
public interface ICommand<TResponse> : IBaseCommand, IRequest<Result<TResponse>> { }

public interface ICommandHandler<in TCommand> : IRequest ...
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
```

- `IBaseCommand` exists purely so pipeline behaviors (e.g. in Mediator) can define one generic constraint that covers both `ICommand` and `ICommand<TResponse>`.
- `TCommand` marked **contravariant** (`in TCommand`) on handler interfaces — needed to assign a handler accepting a base command type to a delegate expecting a more derived type.
- Worked example: `StartFollowingCommand(UserId, FollowedUserId) : ICommand`, record type (immutable, primary constructor). Handler (`StartFollowingCommandHandler`) flow:
  1. Fetch both users via `IUserRepository.GetByIdAsync` (nullable return).
  2. If either is null → return `UserErrors.NotFound(userId)` (a static error factory on a `UserErrors` static class), which converts implicitly to a failure `Result` via an implicit operator on `Error`.
  3. Delegate business logic to a **domain service** (`FollowerService.StartFollowingAsync(user, followedUser, ct)`), which itself returns a `Result`.
  4. If that fails, propagate the failure `Result`.
  5. Otherwise commit via `IUnitOfWork.SaveChangesAsync()` (interface intentionally shaped to mirror EF Core's `DbContext.SaveChangesAsync`) and return `Result.Success()`.

## Implementation Steps / Gotchas & Tips

- Domain logic belongs in domain entities/domain services, not in the command handler — the handler orchestrates (fetch → delegate to domain → persist).
- Command handlers built this way have **no dependency on Mediator or any external library** — pure DI — and are highly unit-testable (mock repository + services with Moq/NSubstitute, verify business rules without a running app).
- Even without Mediator, prefer it anyway for its **pipeline behaviors** (middleware-style cross-cutting concerns — validation, logging, etc., covered in the validation video).
- For queries, pick either Dapper+raw SQL or EF Core+LINQ projection; don't route reads through repositories/domain model — that adds unnecessary overhead for a read-only path.
