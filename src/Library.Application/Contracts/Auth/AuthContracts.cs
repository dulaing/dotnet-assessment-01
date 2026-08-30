namespace Library.Application.Contracts.Auth
{
    public record LoginRequest(string Email, string Password);

    public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string Role, int? MemberId);
}