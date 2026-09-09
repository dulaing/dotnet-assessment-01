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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .AllowAnonymous();

            // Returns the persisted identity represented by the caller's access token.
            group.MapGet("/me", async (AuthService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.GetCurrentAsync(ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("GetCurrentUser")
            .Produces<CurrentUserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
        }
    }
}
