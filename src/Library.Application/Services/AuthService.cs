using Library.Application.Common;
using Library.Application.Contracts.Auth;
using Library.Application.Interfaces;
using Library.Domain.Common;
using Library.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Library.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;
        private readonly IMemberRepository _members;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenGenerator _tokens;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ILogger<AuthService> _logger;
        private readonly ICurrentUser _currentUser;

        public AuthService(
            IUserRepository users,
            IMemberRepository members,
            IPasswordHasher hasher,
            ITokenGenerator tokens,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenRepository refreshTokens,
            ILogger<AuthService> logger,
            ICurrentUser currentUser)
        {
            _users = users;
            _members = members;
            _hasher = hasher;
            _tokens = tokens;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokens = refreshTokens;
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

            var activeAccount = await EnsureAccountIsActiveAsync(user, cancellationToken);
            if (!activeAccount.IsSuccess)
            {
                return activeAccount.ToFailure<LoginResponse>();
            }

            var accessToken = _tokens.Generate(user);
            var refreshToken = _refreshTokenGenerator.Generate();
            await StoreRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

            _logger.LogInformation("User {UserId} signed in with role {Role}", user.Id, user.Role);

            return Result<LoginResponse>.Success(
                new LoginResponse(
                    user.Id,
                    accessToken.Token,
                    accessToken.ExpiresAtUtc,
                    refreshToken.Token,
                    refreshToken.ExpiresAtUtc,
                    user.Role.ToString(),
                    user.MemberId));
        }

        // Rotates a valid refresh token and returns a new access and refresh token pair.
        public async Task<Result<RefreshTokenResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var tokenHash = _refreshTokenGenerator.Hash(request.RefreshToken);
            var storedToken = await _refreshTokens.GetByHashAsync(tokenHash, cancellationToken);
            var now = DateTime.UtcNow;
            if (storedToken is null || !storedToken.IsActiveAt(now))
            {
                return Result<RefreshTokenResponse>.Unauthorized("invalid_refresh_token", "The refresh token is invalid or expired.");
            }

            var user = await _users.GetByIdAsync(storedToken.UserId, cancellationToken);
            if (user is null)
            {
                return Result<RefreshTokenResponse>.Unauthorized("account_not_found", "The account no longer exists.");
            }

            var activeAccount = await EnsureAccountIsActiveAsync(user, cancellationToken);
            if (!activeAccount.IsSuccess)
            {
                return activeAccount.ToFailure<RefreshTokenResponse>();
            }

            var nextRefreshToken = _refreshTokenGenerator.Generate();
            storedToken.Revoke(now, nextRefreshToken.Hash);
            await AddRefreshTokenAsync(user.Id, nextRefreshToken, cancellationToken);
            await _refreshTokens.SaveChangesAsync(cancellationToken);

            var accessToken = _tokens.Generate(user);
            _logger.LogInformation("User {UserId} rotated a refresh token", user.Id);

            return Result<RefreshTokenResponse>.Success(
                new RefreshTokenResponse(
                    user.Id,
                    accessToken.Token,
                    accessToken.ExpiresAtUtc,
                    nextRefreshToken.Token,
                    nextRefreshToken.ExpiresAtUtc,
                    user.Role.ToString(),
                    user.MemberId));
        }

        // Revokes the presented refresh token and stays idempotent for unknown tokens.
        public async Task<Result<bool>> LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var tokenHash = _refreshTokenGenerator.Hash(request.RefreshToken);
            var storedToken = await _refreshTokens.GetByHashAsync(tokenHash, cancellationToken);
            if (storedToken is null)
            {
                return Result<bool>.Success(true);
            }

            if (storedToken.RevokedAtUtc is null)
            {
                storedToken.Revoke(DateTime.UtcNow);
                await _refreshTokens.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Refresh token {RefreshTokenId} was revoked during logout", storedToken.Id);
            return Result<bool>.Success(true);
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

        // Applies the member activation rule to both login and token rotation.
        private async Task<Result<bool>> EnsureAccountIsActiveAsync(User user, CancellationToken cancellationToken)
        {
            if (user.MemberId is null)
            {
                return Result<bool>.Success(true);
            }

            var member = await _members.GetByIdAsync(user.MemberId.Value, cancellationToken);
            if (member is not null && member.IsActive)
            {
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Inactive member account {UserId} attempted to authenticate", user.Id);
            return Result<bool>.Forbidden("account_inactive", "This member account is inactive.");
        }

        // Persists a newly issued refresh-token hash during login.
        private async Task StoreRefreshTokenAsync(int userId, GeneratedRefreshToken token, CancellationToken cancellationToken)
        {
            await AddRefreshTokenAsync(userId, token, cancellationToken);
            await _refreshTokens.SaveChangesAsync(cancellationToken);
        }

        // Adds a token entity without persisting the raw secret.
        private Task AddRefreshTokenAsync(int userId, GeneratedRefreshToken token, CancellationToken cancellationToken) =>
            _refreshTokens.AddAsync(new RefreshToken
            {
                UserId = userId,
                TokenHash = token.Hash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = token.ExpiresAtUtc
            }, cancellationToken);
    }
}
