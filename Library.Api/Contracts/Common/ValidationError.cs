namespace Library.Api.Contracts.Common;

public sealed class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
