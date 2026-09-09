using Library.Api.Extensions;
using Library.Application.Contracts.Borrowings;
using Library.Application.Services;
using Library.Api.Filters;
using Library.Api.Security;

namespace Library.Api.Endpoints
{
    public static class BorrowingEndpoints
    {
        public static void MapBorrowingEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/borrowings").WithTags("Borrowings");

            group.MapGet("", async (BorrowingService service, CancellationToken ct) =>
                Results.Ok(await service.GetAllAsync(ct)))
                .WithName("GetBorrowings")
                .Produces<List<BorrowingResponse>>()
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .RequireAuthorization(AuthorizationPolicies.AdminOnly);

            group.MapPost("", async (CreateBorrowingRequest request, BorrowingService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.BorrowAsync(request, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/borrowings/{result.Value!.Id}", result.Value)
                    : result.ToProblem(http);
            })
            .WithName("BorrowBook")
            .Produces<BorrowingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization();

            group.MapPost("/{id:int}/return", async (int id, BorrowingService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.ReturnAsync(id, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("ReturnBook")
            .Produces<BorrowingResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .AddEndpointFilter<ValidationFilter<CreateBorrowingRequest>>()
            .RequireAuthorization();

            // sits under the member url but returns borrowings, so it is mapped with them
            app.MapGet("/api/members/{memberId:int}/borrowings", async (int memberId, BorrowingService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.GetByMemberIdAsync(memberId, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithTags("Borrowings")
            .WithName("GetBorrowingsByMember")
            .Produces<List<BorrowingResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization();
        }
    }
}
