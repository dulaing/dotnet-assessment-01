# Refactoring From Transaction Script to Domain-Driven Design

- **Video:** https://youtube.com/watch?v=KTSpDZNfjhU
- **Duration:** 15:22
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 10/22

## Key Learnings

Walkthrough of refactoring an `AddExercises` command handler from the **Transaction Script pattern** (all logic procedurally inline in the handler) to the **Domain Model pattern** (behavior pushed down into entities). Goal: more maintainable, more testable, better encapsulated business logic.

## Concepts & Patterns

- **Transaction Script starting point**: handler does everything as ordered procedural steps — (1) fetch workout via repository, (2) loop over exercise requests validating & constructing `Exercise` objects inline (checking `TargetType == Distance` requires `DistanceInMeters`, `TargetType == Time` requires `DurationInSeconds`, collecting `Error`s into a list), (3) insert exercises into repository, (4) `unitOfWork.SaveChangesAsync()`.
- **Refactor step 1 — Static Factory pattern on `Exercise`**: move creation + validation into `Exercise.Create(workoutId, exerciseType, targetType, distance, duration) : Result<Exercise>`, returning `Result.Failure<Exercise>(ExerciseErrors.MissingDistance)` / `MissingDuration` instead of throwing or collecting into an external list. Making the constructor **private** forces all callers through the validated factory method.
- **Refactor step 2 — Encapsulate the collection**: `Workout` gets a private backing field `List<Exercise> _exercises` (property returns `.ToList()` so callers can't mutate it directly), plus a new `Workout.AddExercise(...) : Result` method that internally calls `Exercise.Create`, appends to `_exercises` on success, and propagates failure. This removes the public settable exercises collection — no other code can add/remove exercises except through this guarded method.
- **Refactor step 3 — Aggregate error collection with LINQ**: replace manual for-loop error accumulation with `ValidationError.FromResults(results)` — a factory method that inspects a collection of `Result` objects, extracts failures. Combined with LINQ: `var results = exerciseRequests.Select(r => workout.AddExercise(...)).ToList();` then `if (results.Any(r => r.IsFailure)) return ValidationError.FromResults(results);`
- **Refactor step 4 — batch repository call**: rename `Insert` → `InsertRange(IEnumerable<Exercise>)` on the repository (backed by EF Core's `AddRange`), replacing the one-by-one insert loop with a single call.
- End state: handler becomes short — fetch workout, `workout.AddExercise(...)` per request (or via LINQ), check for validation failures, `repository.InsertRange(...)`, `SaveChangesAsync()`. The domain now owns the business rules; the handler is orchestration only.

## Gotchas & Tips

- **Primary constructors (.NET 8)** used throughout for handler dependency injection — no more manual `private readonly` fields.
- Key testability win: with logic in the domain (`Exercise.Create`, `Workout.AddExercise`), you can unit-test the domain model directly without going through the full use case / mocking repositories and unit of work — you can assert on side effects like "was the exercise added to the internal collection."
- With Transaction Script, testing behavior requires exercising the whole handler and mocking all its dependencies just to reach the logic you care about.
- Trade-off note: this is a gradual refactor ("one step at a time") rather than a rewrite — useful as a template for incrementally migrating existing procedural handlers.
