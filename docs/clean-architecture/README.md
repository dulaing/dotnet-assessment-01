# Clean Architecture & DDD — Study Notes

Condensed learning notes from the YouTube playlist [Learn Clean Architecture & Domain-Driven Design](https://youtube.com/playlist?list=PLYpjLpq5ZDGv370qMB4PLF-PlGdBhP0PA) by **Milan Jovanović** (22 videos). Each note captures the concrete technical substance of its video — patterns, class/interface names, implementation steps, trade-offs — rather than a verbatim transcript.

## Foundations

1. [Clean Architecture Project Setup From Scratch With .NET 7](01-clean-architecture-project-setup-dotnet7.md) — 4-layer solution structure (Domain/Application/Infrastructure/Presentation), dependency rule, per-layer DI extension methods.
2. [Clean Architecture With .NET 6 And CQRS - Project Setup](02-clean-architecture-cqrs-project-setup-dotnet6.md) — .NET 6 variant of the same setup, keeping controllers in a separate Presentation project.
14. [Onion Architecture vs Clean Architecture Comparison](14-onion-vs-clean-architecture.md) — how the two styles relate and where they differ.
22. [The Beginner's Guide to Clean Architecture](22-beginners-guide-to-clean-architecture.md) — high-level primer tying the layers and patterns together.

## CQRS & MediatR

3. [CQRS Doesn't Have To Be Complicated](03-cqrs-doesnt-have-to-be-complicated.md) — CQRS without a separate read/write database, commands vs. queries.
4. [How to Implement the CQRS Pattern in Clean Architecture (from scratch)](04-implement-cqrs-pattern-from-scratch.md) — wiring MediatR-style commands/queries/handlers into the layers.
5. [How To Implement Validation With MediatR And FluentValidation](05-validation-with-mediatr-fluentvalidation.md) — pipeline behaviors for cross-cutting validation.
9. [Using Separate Read/Write Models with EF Core and CQRS](09-separate-read-write-models-efcore-cqrs.md) — dedicated read models/projections vs. write-side entities.

## Error Handling & Resilience

6. [Get Rid of Exceptions in Your Code With the Result Pattern](06-result-pattern-instead-of-exceptions.md) — `Result`/`Result<T>` for expected failures instead of exceptions.
7. [The New Global Error Handling in ASP.NET Core 8](07-global-error-handling-aspnet-core-8.md) — `IExceptionHandler` and ASP.NET Core 8's built-in global handling.
8. [Transactional Outbox Pattern](08-transactional-outbox-pattern.md) — reliably publishing events/messages alongside a DB transaction.

## Architecture Styles & Refactoring

10. [Refactoring From Transaction Script to Domain-Driven Design](10-refactoring-transaction-script-to-ddd.md) — moving logic out of handlers into the domain model.
11. [Vertical Slice Architecture Project Setup From Scratch](11-vertical-slice-architecture-setup.md) — feature-folder alternative to layered Clean Architecture.
12. [CRUD REST API With Clean Architecture & DDD In .NET 7](12-crud-rest-api-clean-architecture-ddd.md) — end-to-end CRUD endpoint through all layers.
13. [Adding Filtering, Sorting And Pagination To a REST API](13-filtering-sorting-pagination-rest-api.md) — query-parameter patterns for list endpoints.

## Domain-Driven Design

15. [How To Use Domain-Driven Design In Clean Architecture](15-domain-driven-design-in-clean-architecture.md) — anemic → rich domain model refactor (constructors, static factories, encapsulation).
16. [What Is An Entity?](16-what-is-an-entity-ddd.md) — entity identity vs. value equality.
17. [How to Use Value Objects to Solve Primitive Obsession](17-value-objects-primitive-obsession.md) — replacing primitives with domain-meaningful types.
18. [Aggregate Root Design 101](18-aggregate-root-design-101.md) — aggregate boundaries and consistency rules.
19. [Domain Validation With .NET](19-domain-validation-with-dotnet.md) — where and how to enforce invariants in the domain layer.
21. [Using Domain Events To Build A Decoupled System](21-domain-events-decoupled-system.md) — raising and dispatching domain events from entities.

## Persistence

20. [Why I Use The Unit of Work Pattern With EF Core](20-unit-of-work-pattern-with-efcore.md) — transaction boundaries and `SaveChangesAsync` coordination across repositories.

---

*Notes generated from video transcripts via [TranscriptAPI](https://transcriptapi.com); numbering follows playlist order (1–22).*
