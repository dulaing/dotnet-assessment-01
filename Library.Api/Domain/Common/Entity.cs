namespace Library.Api.Domain.Common;

// Base for tables that have a single primary key. The key type is generic so an
// entity can use int, long, or Guid without changing this class.
public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
}
