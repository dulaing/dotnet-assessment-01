# Get Rid of Exceptions in Your Code With the Result Pattern

- **Video:** https://youtube.com/watch?v=WCCkEe_Hy2Y
- **Duration:** 13:06
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 6/22

## Key Learnings

- Two common ways to signal domain validation failures: **throwing exceptions** vs **returning a Result/error object**. Walks through the trade-offs by incrementally refactoring a domain service method (a "follow user" use case) from one style to the other.

## Concepts & Patterns

- **Exceptions approach:** throw specific custom exception types (e.g. `CantFollowYourselfException : Exception`, with a parameterless constructor passing a fixed message to the base). Pros: caller knows exactly what to catch; stack traces give rich debugging context. Cons: proliferation of exception classes as use cases grow (one per validation rule gets unwieldy); performance cost to construct/throw exceptions.
- **String return** (intermediate/rejected step): return an error message string, empty string = success. More performant than exceptions but poor maintainability (no structure, magic empty-string convention).
- **`Error` object:** replaces bare strings.
  ```csharp
  public sealed record Error(string Code, string? Description = null)
  {
      public static readonly Error None = new(string.Empty);
  }
  ```
  Domain errors get documented centrally per aggregate, e.g.:
  ```csharp
  public static class FollowerErrors
  {
      public static readonly Error SameUser = new("Followers.SameUser", "...");
      public static readonly Error NonPublicProfile = new("Followers.NonPublicProfile", "...");
      public static readonly Error AlreadyFollowing = new("Followers.AlreadyFollowing", "...");
  }
  ```
  Benefit: errors are self-documenting and discoverable — a consumer can see every possible error a method can return by looking at the `<Aggregate>Errors` static class.
- **`Result` object:** composes success/failure + an `Error`.
  ```csharp
  public class Result
  {
      public bool IsSuccess { get; }
      public bool IsFailure => !IsSuccess;
      public Error Error { get; }

      protected Result(bool isSuccess, Error error)
      {
          if (isSuccess && error != Error.None)
              throw new ArgumentException("Invalid error for successful result", nameof(error));
          if (!isSuccess && error == Error.None)
              throw new ArgumentException("Invalid error for failure result", nameof(error));

          IsSuccess = isSuccess;
          Error = error;
      }

      public static Result Success() => new(true, Error.None);
      public static Result Failure(Error error) => new(false, error);
  }
  ```
  Private constructor + static factories (`Success()`/`Failure(error)`) enforce invariants: a success result can't carry a real error, a failure result must carry one.
- A **generic `Result<TValue>`** variant lets success paths carry a return value.
- **Implicit conversion sugar:**
  ```csharp
  public static implicit operator Result(Error error) => Result.Failure(error);
  ```
  Lets a method `return FollowerErrors.SameUser;` directly where a `Result` is expected — no need to write `Result.Failure(...)` at every failure site.

## Trade-offs (presenter's explicit comparisons)

| | Exceptions | Result/Error object |
|---|---|---|
| Caller ergonomics | Must catch specific exception types | Just checks `IsSuccess`/`IsFailure` |
| Debuggability | Stack trace included automatically | No stack trace by default (could be added to base `Result`) |
| Performance | Slower — cost to construct & throw | Faster — plain object allocation |
| Maintainability | Exception-class sprawl as use cases grow | Centralized, documented error catalog per aggregate |
| Testability | Assert on thrown exception type | Assert on `Result.Error` / `Result.IsFailure` directly, easy to target specific error codes in unit tests |

## Gotchas & Tips

- The `Result` constructor's validation (throwing `ArgumentException` if success/error state is inconsistent) is what makes the type trustworthy — without it you could accidentally build a "successful" result carrying an error.
- This `Result`/`Error` pattern is the same one used as the return type convention for `ICommand`/`IQuery` handlers in the CQRS videos (see `03-cqrs-doesnt-have-to-be-complicated.md`, `04-implement-cqrs-pattern-from-scratch.md`) and consumed by the validation pipeline behavior (`05-validation-with-mediatr-fluentvalidation.md`).
