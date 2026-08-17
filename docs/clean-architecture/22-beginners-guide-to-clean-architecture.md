# The Beginner's Guide to Clean Architecture

- **Video:** https://youtube.com/watch?v=TQdLgzVk2T8
- **Duration:** 13:19
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 22/22

## Key Learnings

- Clean Architecture is an **opinionated, prescriptive** way to structure code: business rules live at the system's core (`Domain` + `Application` layers); `Presentation` and `Infrastructure` are "details" — external dependencies — though Milan says he takes a more pragmatic view of that "details" framing rather than a purist one.
- Delivers three architectural qualities when done well: **maintainability, testability, loose coupling**.
- Aligned design principles: separation of concerns, encapsulation, **dependency inversion**, explicit dependencies, single responsibility, persistence ignorance.
- **Dependency inversion in practice**: outer layers reference inner layers, never the reverse. `Application → Domain`; `Infrastructure → Application`; `Infrastructure` and `Presentation` may reference each other (same abstraction level, outermost ring). Inner layers define interfaces/abstractions (e.g. a notification-service interface in `Application`); outer layers (`Infrastructure`) implement them, wired up via DI at runtime.
  - Caveat: not all abstractions are equally replaceable in practice — swapping databases is a huge undertaking; swapping an email provider (e.g. Mailchimp → something else) is comparatively trivial. Don't assume DI abstraction = free swap.

## Concepts & Patterns — Layer by layer

- **Domain layer** (core, no outward references): entities with business rules, value objects, domain events, domain services, interfaces needed by the domain (e.g. repository interfaces), domain exceptions, enums.
- **Application layer**: orchestrates the domain — checks preconditions, tells domain objects what to do ("application business logic"). Defines **use cases**, implemented either as application services or (Milan's preferred approach) **CQRS with MediatR/Mediator**.
- **Infrastructure layer**: talks to external systems — databases, messaging (RabbitMQ/SQS), email providers, blob storage, identity providers (Keycloak/Auth0), even the system clock.
- **Presentation layer**: entry point (REST API, gRPC, Blazor, etc.) — takes incoming requests and delegates to the appropriate use case; hosts API endpoints, middleware, DI setup.

### CQRS trade-off (used to implement the Application layer's use cases)
- CQRS is **optional** — plain service classes work fine too.
- Pros: single responsibility per command/query handler, interface segregation, MediatR **pipeline behaviors** give you the Decorator pattern for free (validation, logging, etc.), loose coupling (caller only knows the query/command shape, not the handler).
- Con: "magic" — reflection-based dispatch from message object to handler makes debugging/navigation less direct.

### Milan's concrete query/command flow
- **Query side**: endpoint → query object → query handler → raw SQL via **Dapper** (chosen for performance and shaping a read model exactly matching the query) → DB → result returned straight back up. Business logic here is typically just authorization checks or calculations.
- **Command side**: endpoint → command object → command handler → repository (EF Core, hidden behind a repository abstraction) fetches domain entities → call methods on the domain entities to run business logic → **Unit of Work** (EF Core) persists changes. More layers of indirection, deliberately, to keep business logic encapsulated in the domain and handlers testable.

## Gotchas & Tips

- **When to use Clean Architecture**: works for monoliths, modular monoliths, and microservices alike. Best fit when: (1) you need DDD / have complex business logic — domain-centric structure helps; (2) you need high testability; (3) you want the architecture itself to enforce your team's design policies (it's inherently opinionated).
- Not every project needs this much structure — evaluate against actual complexity/testing needs rather than defaulting to it.
