namespace Library.Application.Common
{
    // Carries the one-time token value plus the hash that is safe to persist.
    public record GeneratedRefreshToken(string Token, string Hash, DateTime ExpiresAtUtc);
}
