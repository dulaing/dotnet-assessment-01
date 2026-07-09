namespace Library.Api.Contracts.Common;

public sealed class ValidationErrorResponse : ErrorResponse
{
    public IReadOnlyList<ValidationError> Errors { get; set; } = [];
}
