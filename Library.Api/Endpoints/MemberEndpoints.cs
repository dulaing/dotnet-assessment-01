using Library.Api.Application.Interfaces;
using Library.Api.Contracts.Common;
using Library.Api.Contracts.Members;
using Library.Api.Infrastructure.Filters;

namespace Library.Api.Endpoints;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        // Lists every registered member.
        group.MapGet(
                string.Empty,
                async (IMemberService memberService, CancellationToken cancellationToken) =>
                    Results.Ok(await memberService.GetAllAsync(cancellationToken)))
            .WithName("GetMembers")
            .Produces<IReadOnlyList<MemberResponse>>(StatusCodes.Status200OK);

        // Fetches one member by its identifier.
        group.MapGet(
                "/{id:int}",
                async (int id, IMemberService memberService, CancellationToken cancellationToken) =>
                    Results.Ok(await memberService.GetByIdAsync(id, cancellationToken)))
            .WithName("GetMemberById")
            .Produces<MemberResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        // Registers a new member with the default active state.
        group.MapPost(
                string.Empty,
                async (CreateMemberRequest request, IMemberService memberService, CancellationToken cancellationToken) =>
                {
                    var member = await memberService.CreateAsync(request, cancellationToken);
                    return Results.Created($"/api/members/{member.Id}", member);
                })
            .Validate<CreateMemberRequest>()
            .WithName("CreateMember")
            .Produces<MemberResponse>(StatusCodes.Status201Created)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        // Updates member details and active status.
        group.MapPut(
                "/{id:int}",
                async (int id, UpdateMemberRequest request, IMemberService memberService, CancellationToken cancellationToken) =>
                    Results.Ok(await memberService.UpdateAsync(id, request, cancellationToken)))
            .Validate<UpdateMemberRequest>()
            .WithName("UpdateMember")
            .Produces<MemberResponse>(StatusCodes.Status200OK)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        // Removes a member when borrowing history does not block deletion.
        group.MapDelete(
                "/{id:int}",
                async (int id, IMemberService memberService, CancellationToken cancellationToken) =>
                {
                    await memberService.DeleteAsync(id, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteMember")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}
