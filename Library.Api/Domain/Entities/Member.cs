namespace Library.Api.Domain.Entities;

public sealed class Member
{
    private DateTime _registeredDate;

    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime RegisteredDate
    {
        get => _registeredDate;
        set => _registeredDate = NormalizeUtc(value);
    }

    public bool IsActive { get; set; }

    private static DateTime NormalizeUtc(DateTime value)
    {
        if (value == default)
        {
            return value;
        }

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value.ToUniversalTime()
        };
    }
}
