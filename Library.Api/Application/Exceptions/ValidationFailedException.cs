using Library.Api.Contracts.Common;

namespace Library.Api.Application.Exceptions;

public sealed class ValidationFailedException(string message, IReadOnlyList<ValidationError> errors) : Exception(message)
{
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}
