# Adding Filtering, Sorting And Pagination To a REST API | .NET 7

- **Video:** https://youtube.com/watch?v=X8zRvXbirMU
- **Duration:** 24:02
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 13/22

## Key Learnings

Extends the `GetProductsQuery` (list endpoint) from the CRUD video to support filtering, dynamic sorting, and pagination, all composed as a single deferred `IQueryable<Product>` before one final materializing call — keeping the actual SQL efficient (filter/sort/paginate all happen in the database, not in memory).

## Concepts & Patterns

- **Deferred `IQueryable` composition**: build up `products.AsQueryable()` (or cast to `IQueryable<Product>`) then conditionally chain `.Where`, `.OrderBy`, `.Skip/.Take` before executing — nothing hits the DB until the final `ToListAsync`/`CountAsync`.
- **`PagedList<T>` envelope**: reusable helper exposing `Items`, `Page`, `PageSize`, `TotalCount`, `HasNextPage` (`Page * PageSize < TotalCount`), `HasPreviousPage` (`Page > 1`). Private constructor + static async factory:

```csharp
public static async Task<PagedList<T>> CreateAsync(
    IQueryable<T> query, int page, int pageSize)
{
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    return new PagedList<T>(items, page, pageSize, totalCount);
}
```
- Query handler return type changes from `List<ProductResponse>` to `PagedList<ProductResponse>`.

## Implementation Steps

1. **Filtering**: add a nullable `SearchTerm` string to `GetProductsQuery`; in the handler, `if (!string.IsNullOrWhiteSpace(SearchTerm))` apply `.Where(p => p.Name.Contains(term) || (string)p.Sku ... )`.
2. **Sorting**: add `SortColumn` and `SortOrder` as separate query-string params (vs. a combined `"name descending"` string) — cleaner to parse, and a default sort is required because pagination needs a deterministic order. Build the key-selector via a `switch` expression on `SortColumn.ToLower()` mapping to a member access (default: `ProductId`; also `sku`, `name`, `price amount`/`amount`, `currency`), then apply `OrderBy`/`OrderByDescending` based on whether `SortOrder` (lowercased) equals `"descending"`.
3. **Pagination**: add `Page` and `PageSize` params; call `PagedList<ProductResponse>.CreateAsync(productResponsesQuery, page, pageSize)` and return that instead of a plain list.

## Gotchas & Tips

- **EF Core cannot translate LINQ over a value object directly** — filtering `sku.Contains(term)` throws at runtime because of how `Sku` is configured (value conversion). Fix: add an **explicit cast operator** on the value object (`public static explicit operator string(Sku sku) => sku.Value;`) and cast it in the query: `((string)p.Sku).Contains(term)`.
- A `Contains` filter causes a full table scan on `Name`/`Sku` — fine for small tables, but consider a DB index if the table is large.
- `Skip`/`Take` math: `Skip((page - 1) * pageSize)`, `Take(pageSize)` — pages are 1-indexed.
- Order of operations in the handler: filter → sort → project → paginate — do this all before calling `ToListAsync`/`CreateAsync` so the DB does the work.
