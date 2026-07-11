namespace Library.Api.Application.Exceptions;

// Signals a valid request that clashes with current business or data state.
public sealed class ConflictException(string message) : Exception(message)
{
}
