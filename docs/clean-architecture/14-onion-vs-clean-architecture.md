# Onion Architecture vs Clean Architecture Comparison

- **Video:** https://youtube.com/watch?v=KqWNtCpjUi8
- **Duration:** 13:44
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 14/22

## Key Learnings

Onion and Clean Architecture are, in the presenter's view, conceptually interchangeable — both enforce a strict inward dependency rule around a domain core, and differ mainly in naming/emphasis (services vs. use cases). Demonstrated side-by-side on the same "Newsletters API" example.

## Concepts & Patterns

**Onion Architecture** (Jeffrey Palermo, 2008) — concentric rings, innermost to outermost:
1. Domain entities (core business logic)
2. Repository interfaces (gateway to entities)
3. Service interfaces (business logic that isn't repository-shaped; may reference repository interfaces)
4. Infrastructure (DB, file/cloud storage) + UI (MVC/Web API) + tests — outermost, can reference any inner layer

Four tenets (Palermo): (1) app is built around an independent domain object model; (2) inner layers define interfaces, outer layers implement them; (3) coupling direction points toward the center; (4) core can be *compiled* independently of infrastructure (though not *run* without implementations — presenter calls this tenet "debatable").

**Clean Architecture** (Robert C. Martin / "Uncle Bob", 2012) — explicitly described as a repackaging of onion/hexagonal ideas:
1. Entities (enterprise business rules)
2. Use cases / Application layer (application business rules; orchestrates entities)
3. Interface adapters (controllers, gateways, presenters)
4. Frameworks & drivers (DB, web API, external interfaces, UI) — outermost

Both enforce the **Dependency Inversion Principle**: dependencies always point inward; outer layers may reference inner layers, never the reverse.

## Implementation Steps (comparison walkthrough)

**Onion example (Newsletters API)**:
- Domain project: `Article` entity + `IArticleRepository` (Insert/GetById/Update).
- Application project: `ArticleService` (plain class, no interface — "self-contained," DI-injected repository) holding business logic (e.g. `CreateArticle`, `PublishArticle`). Also defines a service interface for queries, e.g. `IGetArticleByIdQueryHandler`.
- External layer: Persistence project holds the EF Core `DbContext`, `ArticleRepository` implementation, and `GetArticleByIdQueryHandler` implementation. API/Presentation project wires minimal API endpoints directly against `ArticleService` and the query handler interface.
- DI: services registered as **Scoped** (must match the EF Core `DbContext` scoped lifetime).

**Clean Architecture example (same domain)**:
- Same Domain layer (entity + repository interface) — unchanged.
- Application layer swaps services for **use cases** implemented as MediatR commands/handlers (`CreateArticleCommand`/Handler, `PublishArticleCommand`/Handler) instead of one `ArticleService` class.
- Query implementation still lives in Persistence (`GetArticleByIdQueryHandler`), same as onion, just invoked via MediatR.
- API layer injects `ISender` and sends commands/queries rather than calling a service directly.
- DI: MediatR handlers span multiple assemblies (Application + Persistence), so registration uses `RegisterServicesFromAssemblies(...)` listing both.

## Gotchas & Tips

- You don't need an interface for every class — `ArticleService` is used directly (no `IArticleService`) because it's self-contained and only depends on already-abstracted repositories.
- The "compile without infrastructure" tenet is compile-time only — you still need real implementations to *run* the app.
- Picking onion vs. clean is largely a naming/organizational choice; the load-bearing rule in both is the inward dependency direction.
