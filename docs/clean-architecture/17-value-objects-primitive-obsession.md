# How to Use Value Objects to Solve Primitive Obsession

- **Video:** https://youtube.com/watch?v=P5CRea21R2E
- **Duration:** 13:54
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 17/22

## Key Learnings

- **Primitive obsession**: overusing primitive types (`string`, `int`, `bool`) to represent domain concepts leads to two concrete problems:
  1. **Parameter-order bugs** — a constructor like `Member(string firstName, string lastName, string email)` compiles fine even if you accidentally swap arguments (e.g. pass email where firstName goes); the compiler can't catch it.
  2. **No place to enforce constraints** — a raw `string` can't enforce "max 50 chars" or "must be a valid email format" on its own.
- **Value Objects (DDD)** solve this: a type defined entirely by its values (structural equality — two VOs with identical values are equal) and **immutable by design**.

## Concepts & Patterns

- Build a shared abstract `ValueObject` base class in a `Primitives` folder:
  - Abstract method `IEnumerable<object> GetAtomicValues()` — each concrete VO returns the values that define it (via `yield return`).
  - `Equals(object)` override compares atomic values (`GetAtomicValues().SequenceEqual(other.GetAtomicValues())`).
  - `GetHashCode()` override built with `GetAtomicValues().Aggregate(default(int), HashCode.Combine)`.
  - Implements `IEquatable<ValueObject>`.
- Concrete value object (e.g. `FirstName`) is `public sealed class FirstName : ValueObject` with:
  - A single `Value` property with only a `get` (no setter) → immutability.
  - A **private constructor** — forces creation through a factory method.
  - A `public static Result<FirstName> Create(string firstName)` factory that validates and returns a `Result` (from the Result pattern, covered in the previous video) instead of throwing in the constructor. Milan explicitly avoids throwing exceptions from constructors.
  - Validation inside `Create`: empty/whitespace check → `Result.Failure<FirstName>(FirstNameErrors.Empty)`; length check against a `MaxLength` const (e.g. 50) → `...TooLong` error; otherwise `new FirstName(value)`.

## Implementation Steps

1. Create `Primitives/ValueObject.cs` (abstract base with equality machinery).
2. Create a `ValueObjects` folder in the Domain project; add sealed classes per concept (`FirstName`, `LastName`, `Email`, ...).
3. Replace primitive properties/constructor params on entities (e.g. `Member`) with the VO type instead of `string`.
4. Update callers (e.g. `CreateMemberCommandHandler`): call `FirstName.Create(value)`, check `result.IsFailure` (log/return early), then pass `result.Value` into the entity constructor.
5. Entity constructor now only accepts the VO type — you get compile-time type safety (can no longer pass a raw string / swap args accidentally).

## Gotchas & Tips

- Trade-off is real: value objects buy you type safety, immutability, encapsulated constraints, and structural equality — but at the cost of **increased code complexity** (every creation site now has to unwrap a `Result` and handle failure), and the growth compounds if multiple fields become VOs.
- Not a mandatory default — weigh pros/cons per project before adopting value objects everywhere.
