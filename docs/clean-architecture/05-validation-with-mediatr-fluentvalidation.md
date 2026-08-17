# How To Implement Validation With MediatR And FluentValidation

- **Video:** https://youtube.com/watch?v=85dxwd8HzEk
- **Duration:** 18:33
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 5/22

## Key Learnings

- Scope: **input validation** of request objects hitting the API (not business/domain validation, which belongs in the application/domain layer).
- Implements validation as a **Mediator pipeline behavior** that runs FluentValidation validators automatically for every command/query, short-circuiting the handler and returning a structured validation `Result` on failure.

## Concepts & Patterns

- `IValidationResult` interface: exposes a static `ValidationError` and an `Errors` array. Two implementations: `ValidationResult` (extends base `Result`) and `ValidationResult<TResponse>` (extends generic `Result<TResponse>`), each with a static factory taking an `Error[]`.
- `ValidationPipelineBehavior<TRequest, TResponse>` implements Mediator's `IPipelineBehavior<TRequest, TResponse>` with constraints `where TRequest : IRequest<TResponse> where TResponse : Result` (relies on the earlier `ICommand`/`IQuery` convention that everything returns `Result`).

## Implementation Steps

1. Add `FluentValidation` NuGet package to the Application layer.
2. Inject `IEnumerable<IValidator<TRequest>>` into the pipeline behavior.
3. In `Handle`: if no validators registered, just call `next()` (the handler delegate). Otherwise:
   ```csharp
   var errors = validators
       .Select(v => v.Validate(request))
       .SelectMany(r => r.Errors)
       .Where(f => f is not null)
       .Select(f => new Error(f.PropertyName, f.ErrorMessage))
       .Distinct()
       .ToArray();

   if (errors.Any())
       return CreateValidationResult<TResponse>(errors);

   return await next();
   ```
4. `CreateValidationResult<TResult>` uses reflection to build the right result type: if `TResult == Result`, return `ValidationResult.WithErrors(errors)` cast via `as`; otherwise (generic `Result<T>`), get `ValidationResult<>`'s generic type definition, make it generic over `typeof(TResult).GenericTypeArguments[0]`, and invoke its static `WithErrors` method via reflection.
5. Write validators per FluentValidation convention: `<CommandName>Validator : AbstractValidator<CommandName>`, rules in constructor, e.g. `RuleFor(x => x.Email).NotEmpty(); RuleFor(x => x.FirstName).NotEmpty().MaximumLength(FirstName.MaxLength);`.
6. Wire up in `Program.cs`:
   - `builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));`
   - Install `FluentValidation.DependencyInjectionExtensions`, call `services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);`
7. In the API controller base class, add a `HandleFailure(Result result)` helper using pattern matching:
   - `IsSuccess == true` → throw `InvalidOperationException` (should never be called on success).
   - Result `is IValidationResult` → `BadRequest(CreateProblemDetails(...))`, mapping the `Errors` array into a `ProblemDetails`-shaped response.
   - default → `BadRequest(...)` with the single error.
   Replace ad-hoc `BadRequest(result.Error)` calls in controllers with `HandleFailure(result)` so validation error arrays actually reach the client instead of being collapsed to one error.

## Gotchas & Tips

- Easy trap: returning `BadRequest(result.Error)` directly loses the full `Errors` array on a `ValidationResult` — you must special-case `IValidationResult` to surface all validation failures, not just one.
- Alternative approach (seen elsewhere, not preferred by presenter): throw a `ValidationException` from the pipeline behavior and catch it in a global exception handler, converting to the API error response there.
- Result: a bad request now returns `400` with a `ProblemDetails` body containing an `errors` array with one entry per failed rule (e.g. `FirstName must not be empty`, `LastName must not be empty`).
