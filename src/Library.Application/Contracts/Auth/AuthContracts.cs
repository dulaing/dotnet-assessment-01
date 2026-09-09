namespace Library.Application.Contracts.Auth
{
    public record LoginRequest(string Email, string Password);

    public record LoginResponse(int UserId, string AccessToken, DateTime ExpiresAtUtc, string Role, int? MemberId);

    public record CurrentUserResponse(int UserId, string Email, string Role, int? MemberId);
}
