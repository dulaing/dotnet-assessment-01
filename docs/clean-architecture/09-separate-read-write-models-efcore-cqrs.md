# Using Separate Read/Write Models with EF Core and CQRS

- **Video:** https://youtube.com/watch?v=iKDITShiZy4
- **Duration:** 11:08
- **Playlist:** Learn Clean Architecture & Domain-Driven Design (Milan Jovanović), position 9/22

## Key Learnings

A rich domain model (value objects, encapsulated collections, no public navigation properties) is great for enforcing business rules but painful for writing efficient queries. Solution: run **two separate `DbContext`s over the same underlying tables** — a write context using the real domain entities, and a read context using flat, primitive-typed "read model" classes with free navigation properties, optimized purely for querying.

## Concepts & Patterns

- **`ApplicationWriteDbContext`** (renamed from the original context) — maps the rich domain entities (value objects, `AggregateRoot`, etc.), used for commands and migrations.
- **`ApplicationReadDbContext`** (new, `internal`) — maps flat POCO "read models" (e.g. `UserReadModel { Id, Name, Email, IsProfilePublic }`, `FollowerReadModel { UserId, FollowedUserId, CreatedOnUtc }`) with navigation properties allowed (e.g. `FollowerReadModel.User`, `FollowerReadModel.FollowedUser`, `UserReadModel.Followers` collection) since DDD purity doesn't apply to a read-only projection.
- **Configuration separation**: `IEntityTypeConfiguration<T>` classes split into `Configurations/Write/` and `Configurations/Read/` folders. Since `ApplyConfigurationsFromAssembly` would pick up both, use the **predicate overload**:
  ```csharp
  modelBuilder.ApplyConfigurationsFromAssembly(assembly, WriteConfigurationsFilter);

  private static bool WriteConfigurationsFilter(Type t) =>
      t.FullName?.Contains("Configurations.Write") == true;
  ```
  (mirrored with a `Read` filter for the read context.)
- **Read context tuning**: register with `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` by default (it's read-only, so no change tracking needed, and you can drop `.AsNoTracking()` calls everywhere).
- **Relationship configuration example** (`FollowerReadModelConfiguration`): composite key `(UserId, FollowedUserId)`, `HasOne(f => f.User).WithMany(u => u.Followers).HasForeignKey(f => f.UserId)` with `OnDelete(DeleteBehavior.Cascade)`, plus a second one-to-many for `FollowedUser`.
- Existing queries get repointed from the write context to the read context and simplified (no more digging into value object `.Value` properties, no `.AsNoTracking()` needed).
- Because navigation properties exist on the read side, previously-awkward queries become trivial, e.g. fetching the latest 5 followers with a projection:
  ```csharp
  var latestFollowers = await dbContext.Followers
      .Where(f => f.FollowedUserId == query.UserId)
      .OrderByDescending(f => f.CreatedOnUtc)
      .Take(5)
      .Select(f => new { f.User.Id, f.User.Name, f.User.Email, f.User.IsProfilePublic })
      .ToListAsync(cancellationToken);
  ```

## Gotchas & Tips

- Both contexts point at the **same physical tables** — this is purely a mapping-layer split, not a database split. You could, in principle, diverge the read side further (different tables/views) later.
- Dapper-based queries (e.g. `GetUserByEmail`) are unaffected — this pattern is specifically about EF Core-based read queries.
- Only the write context should own migrations.
