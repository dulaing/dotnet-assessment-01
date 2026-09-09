namespace Library.Application.Contracts.Auth
{
    public record LoginRequest(string Email, string Password);

    public record LoginResponse(
        int UserId,
        string AccessToken,
        DateTime ExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc,
        string Role,
        int? MemberId);

    public record RefreshTokenRequest(string RefreshToken);

    public record RefreshTokenResponse(
        int UserId,
        string AccessToken,
        DateTime ExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc,
        string Role,
        int? MemberId);

    public record CurrentUserResponse(int UserId, string Email, string Role, int? MemberId);
}
