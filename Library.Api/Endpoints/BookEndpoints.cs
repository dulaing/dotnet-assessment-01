using Library.Api.Application.Interfaces;
using Library.Api.Contracts.Books;
using Library.Api.Contracts.Common;
using Library.Api.Infrastructure.Filters;

namespace Library.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books")
            .WithTags("Books");

        group.MapGet(
                string.Empty,
                async (IBookService bookService, CancellationToken cancellationToken) =>
                    Results.Ok(await bookService.GetAllAsync(cancellationToken)))
            .WithName("GetBooks")
            .Produces<IReadOnlyList<BookResponse>>(StatusCodes.Status200OK);

        group.MapGet(
                "/{id:int}",
                async (int id, IBookService bookService, CancellationToken cancellationToken) =>
                    Results.Ok(await bookService.GetByIdAsync(id, cancellationToken)))
            .WithName("GetBookById")
            .Produces<BookResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost(
                string.Empty,
                async (CreateBookRequest request, IBookService bookService, CancellationToken cancellationToken) =>
                {
                    var book = await bookService.CreateAsync(request, cancellationToken);
                    return Results.Created($"/api/books/{book.Id}", book);
                })
            .Validate<CreateBookRequest>()
            .WithName("CreateBook")
            .Produces<BookResponse>(StatusCodes.Status201Created)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPut(
                "/{id:int}",
                async (int id, UpdateBookRequest request, IBookService bookService, CancellationToken cancellationToken) =>
                    Results.Ok(await bookService.UpdateAsync(id, request, cancellationToken)))
            .Validate<UpdateBookRequest>()
            .WithName("UpdateBook")
            .Produces<BookResponse>(StatusCodes.Status200OK)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapDelete(
                "/{id:int}",
                async (int id, IBookService bookService, CancellationToken cancellationToken) =>
                {
                    await bookService.DeleteAsync(id, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteBook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}
