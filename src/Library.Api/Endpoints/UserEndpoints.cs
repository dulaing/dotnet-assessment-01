using Library.Api.Extensions;
using Library.Api.Filters;
using Library.Api.Security;
using Library.Application.Contracts.Users;
using Library.Application.Services;

namespace Library.Api.Endpoints
{
    // Maps administrator-managed login account endpoints.
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users").WithTags("Users");

            // Creates a login account while keeping password hashes out of the response.
            group.MapPost("", async (CreateUserRequest request, UserService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.CreateAsync(request, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/users/{result.Value!.Id}", result.Value)
                    : result.ToProblem(http);
            })
            .WithName("CreateUser")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .AddEndpointFilter<ValidationFilter<CreateUserRequest>>()
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        }
    }
}
