using Library.Application.Contracts.Users;
using Library.Application.Interfaces;
using Library.Domain.Common;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services
{
    // Manages login accounts separately from library member profiles.
    public class UserService
    {
        private readonly IUserRepository _users;
        private readonly IMemberRepository _members;
        private readonly IPasswordHasher _hasher;

        public UserService(IUserRepository users, IMemberRepository members, IPasswordHasher hasher)
        {
            _users = users;
            _members = members;
            _hasher = hasher;
        }

        // Creates an admin account or a member account linked to one member.
        public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                return Result<UserResponse>.Validation("invalid_role", "Role must be Admin or Member.");
            }

            if (role == UserRole.Admin && request.MemberId is not null)
            {
                return Result<UserResponse>.Validation("admin_member_link_forbidden", "Admin accounts cannot be linked to a member.");
            }

            if (role == UserRole.Member && request.MemberId is null)
            {
                return Result<UserResponse>.Validation("member_link_required", "Member accounts must be linked to a member.");
            }

            if (await _users.GetByEmailAsync(request.Email, cancellationToken) is not null)
            {
                return Result<UserResponse>.Conflict("user_email_exists", "An account with this email already exists.");
            }

            if (request.MemberId is not null)
            {
                if (await _members.GetByIdAsync(request.MemberId.Value, cancellationToken) is null)
                {
                    return Result<UserResponse>.NotFound("member_not_found", $"Member {request.MemberId.Value} was not found.");
                }

                if (await _users.GetByMemberIdAsync(request.MemberId.Value, cancellationToken) is not null)
                {
                    return Result<UserResponse>.Conflict("member_account_exists", "This member already has an account.");
                }
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = _hasher.Hash(request.Password),
                Role = role,
                MemberId = request.MemberId
            };

            await _users.AddAsync(user, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            return Result<UserResponse>.Success(ToResponse(user));
        }

        // Maps the persisted account to its safe API representation.
        private static UserResponse ToResponse(User user) =>
            new(user.Id, user.Email, user.Role.ToString(), user.MemberId);
    }
}
