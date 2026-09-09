using Library.Application.Common;
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
        private readonly ICurrentUser _currentUser;

        public AuthService(IUserRepository users, IMemberRepository members, IPasswordHasher hasher, ITokenGenerator tokens, ILogger<AuthService> logger, ICurrentUser currentUser)
        {
            _users = users;
            _members = members;
            _hasher = hasher;
            _tokens = tokens;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var email = EmailNormalizer.Normalize(request.Email);
            var user = await _users.GetByEmailAsync(email, cancellationToken);

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
                new LoginResponse(user.Id, token.Token, token.ExpiresAtUtc, user.Role.ToString(), user.MemberId));
        }

        // Loads the current account from the database instead of trusting display claims alone.
        public async Task<Result<CurrentUserResponse>> GetCurrentAsync(CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            {
                return Result<CurrentUserResponse>.Unauthorized("authentication_required", "A valid bearer token is required.");
            }

            var user = await _users.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
            if (user is null)
            {
                return Result<CurrentUserResponse>.Unauthorized("account_not_found", "The authenticated account no longer exists.");
            }

            return Result<CurrentUserResponse>.Success(
                new CurrentUserResponse(user.Id, user.Email, user.Role.ToString(), user.MemberId));
        }
    }
}
