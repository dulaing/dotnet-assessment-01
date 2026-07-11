using System.ComponentModel.DataAnnotations;

namespace Library.Api.Contracts.Members;

public sealed class CreateMemberRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}
