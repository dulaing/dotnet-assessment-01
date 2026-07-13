namespace Library.Api.Domain.Common;

// The common case: a single-key entity that also carries audit fields.
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
{
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}
