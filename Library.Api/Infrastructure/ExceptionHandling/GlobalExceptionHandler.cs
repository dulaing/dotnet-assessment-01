using System.Diagnostics;
using Library.Api.Application.Exceptions;
using Library.Api.Contracts.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Library.Api.Infrastructure.ExceptionHandling;

// Converts application exceptions into the API's standard error payloads.
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (exception is ValidationFailedException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(
                new ValidationErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = validationException.Message,
                    TraceId = traceId,
                    Errors = validationException.Errors
                },
                cancellationToken);

            return true;
        }

        if (exception is NotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

            await httpContext.Response.WriteAsJsonAsync(
                new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = exception.Message,
                    TraceId = traceId
                },
                cancellationToken);

            return true;
        }

        if (exception is ConflictException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(
                new ErrorResponse
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = exception.Message,
                    TraceId = traceId
                },
                cancellationToken);

            return true;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred.",
                TraceId = traceId
            },
            cancellationToken);

        return true;
    }
}
