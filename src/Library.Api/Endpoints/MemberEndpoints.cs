using Library.Api.Extensions;
using Library.Application.Contracts.Members;
using Library.Application.Services;

namespace Library.Api.Endpoints
{
    public static class MemberEndpoints
    {
        public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/members").WithTags("Members");

            group.MapGet("", async (MemberService service, CancellationToken ct) =>
                Results.Ok(await service.GetAllAsync(ct)))
                .WithName("GetMembers")
                .Produces<List<MemberResponse>>();

            group.MapGet("/{id:int}", async (int id, MemberService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.GetByIdAsync(id, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("GetMemberById")
            .Produces<MemberResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("", async (CreateMemberRequest request, MemberService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.CreateAsync(request, ct);
                return result.IsSuccess
                    ? Results.Created($"/api/members/{result.Value!.Id}", result.Value)
                    : result.ToProblem(http);
            })
            .WithName("CreateMember")
            .Produces<MemberResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapPut("/{id:int}", async (int id, UpdateMemberRequest request, MemberService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.UpdateAsync(id, request, ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem(http);
            })
            .WithName("UpdateMember")
            .Produces<MemberResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:int}", async (int id, MemberService service, HttpContext http, CancellationToken ct) =>
            {
                var result = await service.DeleteAsync(id, ct);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem(http);
            })
            .WithName("DeleteMember")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}