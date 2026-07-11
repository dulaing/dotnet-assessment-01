using Library.Api.Application.Exceptions;
using Library.Api.Application.Interfaces;
using Library.Api.Contracts.Members;
using Library.Api.Domain.Entities;
using Library.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Application.Services;

// Holds member-specific business rules outside the HTTP layer.
public sealed class MemberService(IMemberRepository memberRepository) : IMemberService
{
    public async Task<IReadOnlyList<MemberResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var members = await memberRepository.GetAllAsync(cancellationToken);

        return members.Select(MapResponse).ToArray();
    }

    public async Task<MemberResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Member not found");

        return MapResponse(member);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        await EnsureEmailIsUniqueAsync(normalizedEmail, null, cancellationToken);

        // New members start active so they can use the library right away.
        var member = new Member
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber?.Trim(),
            RegisteredDate = DateTime.UtcNow,
            IsActive = true
        };

        await memberRepository.AddAsync(member, cancellationToken);
        await memberRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(member);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Member not found");

        var normalizedEmail = request.Email.Trim();
        await EnsureEmailIsUniqueAsync(normalizedEmail, id, cancellationToken);

        member.FullName = request.FullName.Trim();
        member.Email = normalizedEmail;
        member.PhoneNumber = request.PhoneNumber?.Trim();
        member.IsActive = request.IsActive;

        memberRepository.Update(member);
        await memberRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(member);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Member not found");

        if (await memberRepository.HasBorrowingsAsync(id, cancellationToken))
        {
            throw new ConflictException("Member cannot be deleted because borrowing history exists");
        }

        memberRepository.Remove(member);

        try
        {
            await memberRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Member cannot be deleted because borrowing history exists");
        }
    }

    private async Task EnsureEmailIsUniqueAsync(string email, int? currentMemberId, CancellationToken cancellationToken)
    {
        var existingMember = await memberRepository.GetByEmailAsync(email, cancellationToken);

        if (existingMember is not null && existingMember.Id != currentMemberId)
        {
            throw new ConflictException("A member with the same email already exists");
        }
    }

    private static MemberResponse MapResponse(Member member)
    {
        return new MemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            PhoneNumber = member.PhoneNumber,
            RegisteredDate = member.RegisteredDate,
            IsActive = member.IsActive
        };
    }
}
