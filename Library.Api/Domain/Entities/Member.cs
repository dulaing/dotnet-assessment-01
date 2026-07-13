using Library.Api.Domain.Common;

namespace Library.Api.Domain.Entities;

public sealed class Member : AuditableEntity<int>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime RegisteredDate { get; set; }
    public bool IsActive { get; set; }
}
