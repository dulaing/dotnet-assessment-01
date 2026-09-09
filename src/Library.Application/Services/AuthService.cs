using Library.Application.Contracts.Auth;
using Library.Application.Interfaces;
using Library.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Library.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;
        private readonly IMemberRepository _members;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenGenerator _tokens;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository users, IMemberRepository members, IPasswordHasher hasher, ITokenGenerator tokens, ILogger<AuthService> logger)
        {
            _users = users;
            _members = members;
            _hasher = hasher;
            _tokens = tokens;
            _logger = logger;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

            // one message for both cases, so nobody can probe which emails exist
            if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed sign in attempt");
                return Result<LoginResponse>.Unauthorized("invalid_credentials", "Email or password is incorrect.");
            }

            if (user.MemberId is not null)
            {
                var member = await _members.GetByIdAsync(user.MemberId.Value, cancellationToken);
                if (member is null || !member.IsActive)
                {
                    _logger.LogWarning("Inactive member account {UserId} attempted to sign in", user.Id);
                    return Result<LoginResponse>.Forbidden("account_inactive", "This member account is inactive.");
                }
            }

            var token = _tokens.Generate(user);

            _logger.LogInformation("User {UserId} signed in with role {Role}", user.Id, user.Role);

            return Result<LoginResponse>.Success(
                new LoginResponse(token.Token, token.ExpiresAtUtc, user.Role.ToString(), user.MemberId));
        }
    }
}
