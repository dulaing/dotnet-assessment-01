# Clean Architecture With .NET 6 And CQRS - Project Setup

- **Video:** https://youtube.com/watch?v=tLk4pZZtiDY
- **Duration:** 12:40
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 2/22

## Key Learnings

- Walkthrough of a populated example (Webinars) showing what actually lives in each Clean Architecture layer, plus the reasoning for keeping controllers in a separate Presentation class library.

## Concepts & Patterns

- **Domain layer** (core, references nothing): entities, aggregates, value objects, domain events, repository/factory interfaces, domain services, custom exceptions. Rule: domain can't reference any outer layer.
- `Entity` base class: entities are equal if they share the same `Id`, regardless of reference equality (standard DDD identity semantics).
- **Application layer**: orchestrates use cases, implemented here via **CQRS** — Command Query Responsibility Segregation, splitting write and read flows into separate paths.
  - Commands live under `Webinars/Commands/<UseCase>` — e.g. `CreateWebinarCommand` (data needed) + `CreateWebinarCommandHandler` (uses repository + unit of work, returns new id).
  - Queries live under `Webinars/Queries/<UseCase>` — e.g. `GetWebinarByIdQuery`. Query handlers bypass repositories/EF and use **raw SQL** for performance since reads just need to project data for display.
  - Cross-cutting concerns via **Mediator pipeline behaviors** (analogous to ASP.NET middleware) — e.g. validating commands before the handler runs, throwing a `ValidationException` on failure (handled by upper layers/middleware).
- **Infrastructure layer**: `DbContext` (EF), repository implementations, migrations, entity configurations. Can be split into `Infrastructure` (external systems: email, queues, storage) + `Persistence` (DB access) sub-layers.
- **Presentation layer**: controllers defined in their own class library (not inside the Web API project).

## Implementation Steps

1. Solution has `Core/` (Domain, Application) and `External/` (Infrastructure, Presentation) folders.
2. Register everything in `Program.cs`/`Startup.cs`: `services.AddControllers()` then `.AddApplicationPart(presentationAssembly)` so ASP.NET picks up controllers that live outside the Web API project.
3. Controllers use `Mapster` to map HTTP request DTOs to commands (e.g. `CreateWebinarRequest` → `CreateWebinarCommand`), then `Send` via Mediator.
4. Add `ExceptionHandlingMiddleware` — wraps execution in try/catch, logs errors, returns a **ProblemDetails**-style error envelope (recommended for standardization).

## Gotchas & Tips

- **Why controllers live outside the Web API project:** the Web API project must reference every other project (Domain, Application, Infrastructure, Presentation) to wire up DI. If controllers were defined inside the Web API itself, they'd have direct access to Infrastructure (e.g. injecting the `DbContext` straight into a controller), silently bypassing CQRS/the application layer. Moving controllers into a separate Presentation library means the Web API project's references don't leak into the controllers themselves, so the architecture is actually enforced rather than just "by convention." This is a mistake more junior engineers make.
- Prefer `ProblemDetails` for API error responses — it's a standardized shape.
