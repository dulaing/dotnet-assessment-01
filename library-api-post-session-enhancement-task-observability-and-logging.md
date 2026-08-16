# Library API Post-Session Enhancement Task

## Purpose

This task extends your existing **Library Management Minimal API** assignment.

The goal is to apply a focused set of improvements based on the session on **exception handling and observability**. This is **not** a full rewrite of the project. Keep the scope practical and implement only what is needed to make failure behavior clearer, API errors more consistent, and the application easier to inspect locally.

This is a **post-session enhancement task**, so it introduces additional expectations beyond the original baseline assignment.

## Objective

Improve the Library API so that:

- expected failures are handled intentionally
- unexpected failures are handled safely in one place
- error responses are consistent and easier for clients to use
- logs, traces, and metrics are visible locally through a lightweight Aspire-based developer setup
- anyone who clones the repository can run the project and inspect its telemetry with minimal setup

For this task, the observability scope is **local developer orchestration and local inspection using Aspire**, not production cloud deployment or production monitoring setup. The goal is to make the application easy to run, inspect, and demonstrate in a local development environment within the available time.

## Required Work

### 1. Add centralized global exception handling

- Unexpected failures must be handled in one place at the application boundary.
- The API must return a sanitized error response for unexpected failures.
- Unexpected failures must be logged once at the boundary.

### 2. Improve exception usage

- Do not use exceptions for expected handled outcomes.
- Expected cases should be handled explicitly and returned as normal API responses.
- Exceptions should still be used for truly unexpected failures and exceptional technical or domain conditions.

Examples of expected handled outcomes:

- `book_not_found`
- `member_not_found`
- `duplicate_isbn`
- `duplicate_email`
- `book_unavailable`
- `member_inactive`
- `borrowing_limit_exceeded`
- `book_already_returned`

### 3. Represent expected borrow and return failures as handled outcomes

- At minimum, this must be applied to borrowing a book and returning a book.
- Expected failures in these flows should be returned as normal handled API responses using suitable status codes such as `400 Bad Request`, `404 Not Found`, or `409 Conflict`, not surfaced as unhandled exceptions.
- These flows should clearly distinguish validation failure, expected business rejection, and unexpected technical failure.

Using a **result pattern** for the borrowing workflow is recommended.

Examples:

- `Result<T>`
- `ApplicationResult<T>`
- another equivalent explicit outcome model

You do **not** need to refactor the entire application to a result pattern. Keep the scope limited to the most important workflow unless you want to go further.

### 4. Standardize error responses

- Use `ProblemDetails` for general API failures.
- Use `ValidationProblemDetails` for validation failures.
- Include a `traceId` in all failure responses.
- Include a stable application error `code` for important handled business and lookup failures.

The goal is to give clients and testers one predictable error contract instead of several unrelated response shapes.

### 5. Add structured logging with proper log levels

- Use `Information` for meaningful successful operations.
- Use `Warning` for expected business rejections.
- Use `Error` for unexpected failures.
- Include useful business identifiers in logs where appropriate, such as `BookId`, `MemberId`, `BorrowingId`, or `Isbn`.
- Do not log the same unexpected failure in multiple layers.

### 6. Add local observability using Aspire

- Set up the application using the recommended local .NET OpenTelemetry approach so telemetry is visible in the Aspire Dashboard.
- Logs, traces, and metrics must all be visible locally.
- The setup should be easy for another engineer to run after cloning the repository.
- Keep this lightweight and practical, focused on local developer observability and local orchestration rather than cloud deployment.
- You do not need to implement production cloud monitoring for this task.
- You do not need to move database, database migrations, or migration worker processes into Aspire if doing so adds unnecessary complexity.

### 7. Add health endpoints

- `/alive`
  Liveness check. Answers: "Is this process up and should it stay running?" It should be fast, simple, and should not depend on every external dependency.

- `/health`
  Readiness or service health check. Answers: "Is this application ready to serve requests correctly?" It can include checks that matter for serving traffic, such as required dependencies or configuration, but should still remain fast and deterministic.

### 8. Update documentation

- Document how to run the application locally.
- Document how to start the Aspire setup.
- Document how to open the Aspire Dashboard.
- Document what logs, traces, and metrics a reviewer should expect to see.
- Document the `/alive` and `/health` endpoints and explain what each one represents.
- Document a few success and failure scenarios and the expected API behavior.
- Document how a reviewer can verify that error responses include `traceId` and stable error `code` values.

## Engineering Expectations

Apply the patterns and practices discussed in the exception handling and observability session, especially around intentional failure handling, disciplined exception usage, structured logging, and local OpenTelemetry-based observability.

- Treat failures intentionally by distinguishing validation failures, expected business failures, and unexpected technical failures.
- Catch exceptions only where the code can translate, recover, clean up, or handle the application boundary.
- Do not add unnecessary `try/catch` blocks in every layer.
- Unexpected failures should be logged once at the boundary.
- Avoid logging sensitive or unnecessary personal data.
- Do not use high-cardinality values such as `traceId`, `email`, or `memberId` as metric tags.

## Testing Expectation

Do not expand tests broadly for this task. Only add or update the **most critical tests affected by your changes**, especially where the new failure-handling behavior or error response contract changes existing behavior. Broad test expansion is not expected for this task.

## Not Required

The following are **not required** for this task:

- Azure deployment
- Azure Monitor / Application Insights / Log Analytics
- alert rules
- telemetry retention or access-control design
- migration worker
- full infrastructure redesign
- large-scale test expansion

## Optional Bonus Work

If you complete the main task and still have time, you may go a little further with one or more of the following:

- Add a small set of custom metrics for key library operations, such as borrow attempts, borrow completions, borrow rejections, and successful returns.
- Add custom trace spans only where automatic instrumentation is not enough to explain an important workflow clearly.
- Add a custom exception type only if it represents a genuinely exceptional and meaningful domain or application condition, not a normal business rejection.

## Definition of Done

Your submission is complete when:

- unexpected failures are handled centrally
- expected borrowing and return failures do not surface as `500` errors
- error responses are consistent and standardized
- logs use reasonable levels and useful structured properties
- logs, traces, and metrics are visible in Aspire locally
- `/alive` and `/health` work as described above
- another engineer can clone the repository, run it locally, open Aspire, and understand the available telemetry from your documentation
