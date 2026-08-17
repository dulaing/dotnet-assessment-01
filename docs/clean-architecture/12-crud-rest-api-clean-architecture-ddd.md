# CRUD REST API With Clean Architecture & DDD In .NET 7

- **Video:** https://youtube.com/watch?v=nE2MjN54few
- **Duration:** 29:51
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 12/22

## Key Learnings

Builds full CRUD for a `Product` entity across Domain / Application / Persistence / API projects, demonstrating the "bottom-up" workflow: repository interface → command/query → handler → endpoint. Reinforces the CQRS split — commands go through the rich domain model + repository, queries bypass the domain and hit `DbContext` directly for performance.

## Concepts & Patterns

- **Strongly-typed IDs & value objects**: `ProductId` (record wrapping `Guid`), `Money` (record: `Currency` + `Amount`), `Sku` (value object with a static factory `Sku.Create(...)` enforcing an 8-character constraint).
- **EF Core configuration** (`ProductConfiguration`, Persistence project): `HasKey` on the strongly-typed ID with an explicit value converter to/from `Guid`; simple value conversion for `Sku` (string ↔ value object); **owned entity** mapping for `Money` since it has two properties (Currency, Amount become individual columns).
- **Repository pattern (domain-owned interface)**: `IProductRepository` defined in the Domain project — `Add(Product)`, `Remove(Product)`, `GetByIdAsync(ProductId)`.
- **Unit of Work**: `IUnitOfWork` injected and implemented by the EF Core `DbContext`; call `SaveChangesAsync(cancellationToken)` to persist.
- **CQRS with MediatR**: commands implement `IRequest` (no return) or `IRequest<TResponse>`; handlers implement `IRequestHandler<TCommand>`.
- **Encapsulation**: entity properties have private setters; state changes only happen through constructors, static factories, or explicit methods (e.g. `product.Update(name, money, sku)`).
- **Custom domain exceptions**: `ProductNotFoundException` lives in the Domain project's feature folder, extends `Exception`, takes the ID in its constructor to build the message.
- **Minimal APIs via Carter**: endpoints grouped in an `IEndpointRouteBuilder` extension class.

## Implementation Steps

1. Define `IProductRepository` in Domain with just the method needed for the first feature.
2. Application layer: per-feature folders (Create/Delete/Update/Get) under `Products`.
3. **Create**: `CreateProductCommand` (record, `IRequest`) → handler builds `Product` via a constructor, calls `repository.Add`, then `unitOfWork.SaveChangesAsync`. Endpoint: `MapPost("products", ...)` binding the command directly as the request body, returns `Results.Ok()`.
4. **Delete**: `DeleteProductCommand(ProductId)` → handler does `GetByIdAsync`, null-checks and throws `ProductNotFoundException`, else `repository.Remove` + save. Endpoint: `MapDelete("products/{id:guid}", ...)` wrapped in try/catch → `Results.NotFound(ex.Message)` or `Results.NoContent()`.
5. **Update**: handler fetches the product, throws if missing, calls a dedicated `product.Update(name, money, sku)` method (keeps mutation encapsulated/testable) instead of setting properties directly. Note: explicit `repository.Update()` call is technically redundant because EF Core change-tracks the loaded entity — included "for completeness." Endpoint uses a separate `UpdateProductRequest` DTO (no ID) bound `[FromBody]`, with the ID coming from the route — avoids exposing/duplicating the ID in the body. Returns `204 NoContent`.
6. **Get (query)**: `GetProductQuery(ProductId) : IRequest<ProductResponse>` — handler injects `IApplicationDbContext` directly (not the repository) and projects straight to a flat `ProductResponse` record (primitives only: Guid, string, string, string, decimal). Naming convention used: `...Request` for inbound DTOs, `...Response` for outbound. Endpoint returns `200 OK` with the response or `404` via the same not-found exception pattern.

## Gotchas & Tips

- Private setters will break naive object construction — you must add a constructor (or factory) that accepts all required fields.
- Queries should **not** go through repositories/domain — use the DbContext directly for simpler, more performant reads; commands should go through the domain for correctness/invariants.
- Prefer `FirstOrDefaultAsync` over `SingleOrDefaultAsync` for primary-key lookups (faster, DB already guarantees uniqueness).
- Use a `[FromBody]` attribute explicitly when splitting route/body binding on the same endpoint for clarity.
