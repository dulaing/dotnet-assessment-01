namespace Library.Application.Common
{
    public record AccessToken(string Token, DateTime ExpiresAtUtc);
}