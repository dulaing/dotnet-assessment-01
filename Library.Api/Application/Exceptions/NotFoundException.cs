namespace Library.Api.Application.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
}
