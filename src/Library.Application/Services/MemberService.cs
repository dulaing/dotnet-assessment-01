using Library.Application.Common;
using Library.Application.Contracts.Members;
using Library.Application.Interfaces;
using Library.Domain.Common;
using Library.Domain.Entities;

namespace Library.Application.Services
{
    // member crud, with unique email and history-safe deletes
    public class MemberService
    {
        private readonly IMemberRepository _members;
        private readonly IBorrowingRepository _borrowings;
        private readonly ICurrentUser _currentUser;

        public MemberService(IMemberRepository members, IBorrowingRepository borrowings, ICurrentUser currentUser)
        {
            _members = members;
            _borrowings = borrowings;
            _currentUser = currentUser;
        }

        public async Task<List<MemberResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            var members = await _members.GetAllAsync(cancellationToken);
            return members.Select(ToResponse).ToList();
        }

        public async Task<Result<MemberResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (!CanAccess(id))
            {
                return Result<MemberResponse>.Forbidden("member_access_forbidden", "You cannot access another member's profile.");
            }

            var member = await _members.GetByIdAsync(id, cancellationToken);
            if (member is null)
            {
                return Result<MemberResponse>.NotFound("member_not_found", $"Member {id} was not found.");
            }

            return Result<MemberResponse>.Success(ToResponse(member));
        }

        public async Task<Result<MemberResponse>> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken)
        {
            var email = EmailNormalizer.Normalize(request.Email);
            var existing = await _members.GetByEmailAsync(email, cancellationToken);
            if (existing is not null)
            {
                return Result<MemberResponse>.Conflict("email_already_exists", $"A member with email {request.Email} already exists.");
            }

            // new members join active, and the join date is ours to stamp not the caller's to claim
            var member = new Member
            {
                FullName = request.FullName,
                Email = email,
                PhoneNumber = request.PhoneNumber,
                RegisteredDate = DateTime.UtcNow,
                IsActive = true
            };

            await _members.AddAsync(member, cancellationToken);
            await _members.SaveChangesAsync(cancellationToken);

            return Result<MemberResponse>.Success(ToResponse(member));
        }

        public async Task<Result<MemberResponse>> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken)
        {
            if (!CanAccess(id))
            {
                return Result<MemberResponse>.Forbidden("member_access_forbidden", "You cannot update another member's profile.");
            }

            var member = await _members.GetByIdAsync(id, cancellationToken);
            if (member is null)
            {
                return Result<MemberResponse>.NotFound("member_not_found", $"Member {id} was not found.");
            }

            if (!_currentUser.IsAdmin && request.IsActive != member.IsActive)
            {
                return Result<MemberResponse>.Forbidden("member_status_forbidden", "Members cannot change their account status.");
            }

            var email = EmailNormalizer.Normalize(request.Email);
            var emailOwner = await _members.GetByEmailAsync(email, cancellationToken);
            if (emailOwner is not null && emailOwner.Id != id)
            {
                return Result<MemberResponse>.Conflict("email_already_exists", $"A member with email {request.Email} already exists.");
            }

            member.FullName = request.FullName;
            member.Email = email;
            member.PhoneNumber = request.PhoneNumber;
            member.IsActive = request.IsActive;

            _members.Update(member);
            await _members.SaveChangesAsync(cancellationToken);

            return Result<MemberResponse>.Success(ToResponse(member));
        }

        public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var member = await _members.GetByIdAsync(id, cancellationToken);
            if (member is null)
            {
                return Result<bool>.NotFound("member_not_found", $"Member {id} was not found.");
            }

            if (await _borrowings.ExistsForMemberAsync(id, cancellationToken))
            {
                return Result<bool>.Conflict("member_has_borrowings", $"Member {id} has borrowing history and cannot be deleted.");
            }

            _members.Remove(member);
            await _members.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        // Admins can access every member while members can access only their own record.
        private bool CanAccess(int memberId) =>
            _currentUser.IsAdmin || _currentUser.MemberId == memberId;

        private static MemberResponse ToResponse(Member member) => new(
            member.Id,
            member.FullName,
            member.Email,
            member.PhoneNumber,
            member.RegisteredDate,
            member.IsActive);
    }
}
