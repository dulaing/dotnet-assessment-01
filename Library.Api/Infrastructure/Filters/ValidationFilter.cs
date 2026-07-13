using FluentValidation;
using Library.Api.Application.Exceptions;
using Library.Api.Contracts.Common;

namespace Library.Api.Infrastructure.Filters;

// Runs the FluentValidation validator for the request before the handler executes.
public sealed class ValidationFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();

        if (request is null || validator is null)
        {
            return await next(context);
        }

        var result = await validator.ValidateAsync(request);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .Select(failure => new ValidationError
                {
                    Field = failure.PropertyName,
                    Message = failure.ErrorMessage
                })
                .ToArray();

            throw new ValidationFailedException("Validation failed", errors);
        }

        return await next(context);
    }
}
