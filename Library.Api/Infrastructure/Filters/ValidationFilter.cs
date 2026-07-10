using System.ComponentModel.DataAnnotations;
using Library.Api.Application.Exceptions;
using Library.Api.Contracts.Common;

namespace Library.Api.Infrastructure.Filters;

public sealed class ValidationFilter<TRequest> : IEndpointFilter
    where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return await next(context);
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .SelectMany(
                    result => result.MemberNames.DefaultIfEmpty(string.Empty),
                    (result, memberName) => new ValidationError
                    {
                        Field = memberName,
                        Message = result.ErrorMessage ?? "Validation failed."
                    })
                .ToArray();

            throw new ValidationFailedException("Validation failed", errors);
        }

        return await next(context);
    }
}
