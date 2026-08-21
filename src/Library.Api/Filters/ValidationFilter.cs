using System.Diagnostics;
using FluentValidation;

namespace Library.Api.Filters
{
    // runs the registered validator for a request body before the handler ever sees it
    public class ValidationFilter<T> : IEndpointFilter where T : class
    {
        private readonly IValidator<T> _validator;

        public ValidationFilter(IValidator<T> validator)
        {
            _validator = validator;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var request = context.Arguments.OfType<T>().FirstOrDefault();
            if (request is null)
            {
                return await next(context);
            }

            var result = await _validator.ValidateAsync(request, context.HttpContext.RequestAborted);
            if (result.IsValid)
            {
                return await next(context);
            }

            return Results.ValidationProblem(
                result.ToDictionary(),
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "validation_failed",
                    ["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
                });
        }
    }
}