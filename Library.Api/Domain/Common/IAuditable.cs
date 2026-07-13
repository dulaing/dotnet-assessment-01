namespace Library.Api.Domain.Common;

// Marks an entity that records who created and last changed it, and when.
// Kept separate from Entity<TId> so a keyless or composite-key table can still be audited.
public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
    string? UpdatedBy { get; set; }
}
