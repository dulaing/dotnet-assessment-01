using Library.Api.Extensions;
using Library.Api.Filters;
using Library.Application.Contracts.Auth;
using Library.Application.Services;

namespace Library.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Auth");

            group.MapPost("/login", async (LoginRequest request, AuthService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.LoginAsync(request, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("Login")
            .AddEndpointFilter<ValidationFilter<LoginRequest>>()
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}