using System.Diagnostics;
using Library.Domain.Common;

namespace Library.Api.Extensions
{
    // one place that turns a failed Result into an http response, so endpoints never hand-build errors
    public static class ResultExtensions
    {
        public static IResult ToProblem<T>(this Result<T> result, HttpContext httpContext)
        {
            var error = result.Error!;

            // _=> is the catch all, if someone adds a fourth error, it fails loudly as a 500 instead of returning nothing
            var status = error.Type switch
            {
                ResultErrorType.NotFound => StatusCodes.Status404NotFound,
                ResultErrorType.Conflict => StatusCodes.Status409Conflict,
                ResultErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            // writes the standard error shape
            // RFC 7807 - the thing your spec calls ProblemDetails
            return Results.Problem(
                detail: error.Message,
                statusCode: status,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code,
                    ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
                });
        }
    }
}