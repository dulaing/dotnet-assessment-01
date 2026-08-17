# Domain Validation With .NET | Clean Architecture, DDD, .NET 6

- **Video:** https://youtube.com/watch?v=KgfzM0QWHrQ
- **Duration:** 12:31
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 19/22

## Key Learnings

Two competing ways to enforce domain invariants inside entity methods (demonstrated on the `Gathering` entity's `Create` and `SendInvitation` methods): **throwing exceptions** vs. **returning `Result` objects**. Neither is "correct" — pick based on trade-offs.

## Concepts & Patterns

### Approach 1 — Custom domain exceptions
- Don't throw the base `Exception`/generic exceptions — create a `DomainException` base class (in an `Exceptions` folder) inheriting from `Exception` with a single constructor.
- Create one **sealed, specific exception per rule**, e.g. `GatheringMaxNumberOfAttendeesIsNowException`, `GatheringInvitationsValidBeforeInHoursIsNullException` — each `: DomainException`.
- Pros: execution stops immediately at the point of failure (defends invariants so an entity is never left in an invalid state), generates a **stack trace** for debugging, specific exception types make error logs self-explanatory.
- Cons: **performance cost** when exceptions are actually thrown.

### Approach 2 — Result objects
- Reuses the `Result` / `Result<T>` type from the earlier "Result pattern" video: `IsSuccess`/`IsFailure` flags + an `Error` (has `Code` and `Message`).
- Method signatures change to return `Result<T>` (e.g. `SendInvitation` returns `Result<Invitation>`), and rule violations become `return Result.Failure<Invitation>(new Error(code, message))` instead of `throw`.
- `Result<T>` defines an implicit operator from `T` so a happy-path return (`return invitation;`) still works.
- **Centralize errors**: instead of `new Error(...)` inline everywhere, define a static catalog:
```csharp
public static class DomainErrors
{
    public static class Gathering
    {
        public static readonly Error InvitingCreator = new("Gathering.InvitingCreator", "...");
        public static readonly Error AlreadyPassed = new("Gathering.AlreadyPassed", "...");
    }
}
```
  Then reference `DomainErrors.Gathering.InvitingCreator`, etc. Benefits: more readable/expressive call sites, and over time this catalog becomes a discoverable "menu" of all domain errors for new team members.
- Pros: explicit method signatures (caller knows it can fail and must handle it), no exception-throwing perf cost, growing error catalog aids discoverability.
- Cons: no stack trace, so pinpointing *where* an error occurred is harder — mitigate with good logging.

## Implementation Steps (Result approach, at a call site)

1. Change entity method return type to `Result<T>`.
2. Replace `throw` statements with `return Result.Failure<T>(DomainErrors.X.Y)`.
3. At the caller (e.g. a command handler): capture the result explicitly (`var invitationResult = gathering.SendInvitation(...)`), check `invitationResult.IsFailure` → log/return, otherwise use `invitationResult.Value`.

## Gotchas & Tips

- Milan personally leans toward Result objects for readability/maintainability but stresses both are valid; team preference and performance sensitivity should decide.
- Whichever approach you choose, apply it consistently across the domain layer rather than mixing arbitrarily.
