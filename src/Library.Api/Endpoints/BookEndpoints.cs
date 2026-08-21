using Library.Api.Extensions;
using Library.Application.Contracts.Books;
using Library.Application.Services;
using Library.Api.Filters;

namespace Library.Api.Endpoints
{
    public static class BookEndpoints
    {
        public static void MapBookEndpoints(this IEndpointRouteBuilder app)
        {
            // groups the /api/books heading. Every route below inherits it
            // WithTags is purely for Swagger, it puts them all under one heading.
            var group = app.MapGroup("/api/books").WithTags("Books");

            group.MapGet("", async (BookService service, CancellationToken ct) =>
                Results.Ok(await service.GetAllAsync(ct)))
                .WithName("GetBooks")
                .Produces<List<BookResponse>>();

            // .Produces<...>() is documentation only. It changes nothing at runtime. 
            // It tells Swagger "this returns a BookResponse on success and a problem on 404" so the generated page shows real shapes instead of guessing.
            group.MapGet("/{id:int}", async (int id, BookService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.GetByIdAsync(id, ct);

                //Succeeded, return the value. Failed, hand it to the mapper.
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("GetBookById")
            .Produces<BookResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("", async (CreateBookRequest request, BookService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.CreateAsync(request, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/books/{result.Value!.Id}", result.Value)
                    : result.ToProblem(http);
            })
            .WithName("CreateBook")
            .Produces<BookResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<CreateBookRequest>>();

            group.MapPut("/{id:int}", async (int id, UpdateBookRequest request, BookService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.UpdateAsync(id, request, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("UpdateBook")
            .Produces<BookResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<UpdateBookRequest>>();
            

            group.MapDelete("/{id:int}", async (int id, BookService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.DeleteAsync(id, ct);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem(http);
            })
            .WithName("DeleteBook")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}