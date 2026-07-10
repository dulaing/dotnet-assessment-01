namespace Library.Api.Application.Exceptions;

// Signals that an expected resource could not be found.
public sealed class NotFoundException(string message) : Exception(message)
{
}
