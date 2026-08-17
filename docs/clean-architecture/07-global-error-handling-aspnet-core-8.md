# The New Global Error Handling in ASP.NET Core 8

- **Video:** https://youtube.com/watch?v=uOEDM0c9BNI
- **Duration:** 13:41
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 7/22

## Key Learnings

Two categories of errors need two different mechanisms:
- **Known/expected errors** (validation, not-found, uniqueness conflicts) → **Result pattern**. Predictable, documented as `Error` objects (e.g. a `UserErrors` static class with `NotFound`, `EmailNotUnique`).
- **Unknown/unhandled errors** (bugs, unexpected exceptions) → **exceptions**, caught centrally and turned into a standardized response.

## Concepts & Patterns

- **ProblemDetails (RFC 7807)** — standardize failure responses instead of ad-hoc `BadRequest()`. Set `Status`, `Title`, `Type` (URI to RFC docs), and use `Extensions` (a `Dictionary<string, object>`) to attach an `errors` array built from the `Result`'s `Error`.
- **Result → ProblemDetails extension method**: an extension on `Result` (`ToProblemDetails()`) centralizes the mapping logic; throws `InvalidOperationException` if called on a success result (guard clause).
- **Global exception handling — two approaches:**
  1. **Middleware (classic, works pre-.NET 8)**: convention-based middleware class with `RequestDelegate next` + `ILogger` injected via constructor, `InvokeAsync(HttpContext)` wraps `await _next(context)` in try/catch, logs the exception (structured logging), builds a `ProblemDetails` (500, "Server error"), writes it via `context.Response.WriteAsJsonAsync`. Registered with `app.UseMiddleware<ExceptionHandlingMiddleware>()`.
  2. **`IExceptionHandler` interface (new in .NET 8)**: implement `TryHandleAsync(HttpContext, Exception, CancellationToken)` returning `bool`. Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` + `builder.Services.AddProblemDetails()`, then just `app.UseExceptionHandler()` (no options needed once ProblemDetails service is registered).

## Gotchas & Tips

- If both the custom middleware and an `IExceptionHandler` are registered, whichever is registered/hit **first in the pipeline** wins — `IExceptionHandler` runs through `UseExceptionHandler()`, so ordering relative to `UseMiddleware` matters. Prefer picking one approach (author recommends the new `IExceptionHandler`).
- You can chain multiple `IExceptionHandler` implementations for different exception types; each returns `true` once it has handled the exception, otherwise it bubbles to the next handler (or to an unhandled 500).
- Don't leak exception details (stack traces, messages) in the API response — log them (e.g. to Seq in local dev) but return only the generic ProblemDetails.
- Long-term goal mentioned: eliminate exceptions from app code entirely in favor of the Result pattern everywhere (teased as a follow-up video).
