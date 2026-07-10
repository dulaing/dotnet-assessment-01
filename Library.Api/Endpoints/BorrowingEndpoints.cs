using Library.Api.Application.Interfaces;
using Library.Api.Contracts.Borrowings;
using Library.Api.Contracts.Common;
using Library.Api.Infrastructure.Filters;

namespace Library.Api.Endpoints;

public static class BorrowingEndpoints
{
    public static IEndpointRouteBuilder MapBorrowingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/borrowings")
            .WithTags("Borrowings");

        // Creates a borrowing record when the book and member pass all rules.
        group.MapPost(
                string.Empty,
                async (CreateBorrowingRequest request, IBorrowingService borrowingService, CancellationToken cancellationToken) =>
                {
                    var borrowing = await borrowingService.CreateAsync(request, cancellationToken);
                    return Results.Created($"/api/borrowings/{borrowing.Id}", borrowing);
                })
            .Validate<CreateBorrowingRequest>()
            .WithName("CreateBorrowing")
            .Produces<BorrowingResponse>(StatusCodes.Status201Created)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        // Lists borrowing history across the whole library.
        group.MapGet(
                string.Empty,
                async (IBorrowingService borrowingService, CancellationToken cancellationToken) =>
                    Results.Ok(await borrowingService.GetAllAsync(cancellationToken)))
            .WithName("GetBorrowings")
            .Produces<IReadOnlyList<BorrowingResponse>>(StatusCodes.Status200OK);

        // Marks a borrowing as returned and restores book availability.
        group.MapPost(
                "/{id:int}/return",
                async (int id, IBorrowingService borrowingService, CancellationToken cancellationToken) =>
                    Results.Ok(await borrowingService.ReturnAsync(id, cancellationToken)))
            .WithName("ReturnBorrowedBook")
            .Produces<BorrowingResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        // Lists borrowing history for one specific member.
        app.MapGet(
                "/api/members/{memberId:int}/borrowings",
                async (int memberId, IBorrowingService borrowingService, CancellationToken cancellationToken) =>
                    Results.Ok(await borrowingService.GetByMemberIdAsync(memberId, cancellationToken)))
            .WithTags("Borrowings")
            .WithName("GetBorrowingsByMember")
            .Produces<IReadOnlyList<BorrowingResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
