namespace Library.Api.Contracts.Members;

public sealed class MemberResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime RegisteredDate { get; set; }
    public bool IsActive { get; set; }
}
