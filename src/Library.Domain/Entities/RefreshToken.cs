namespace Library.Domain.Entities
{
    // Stores a hash of an opaque refresh token so leaked database rows are not credentials.
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        // A token can rotate only once and only before its expiry.
        public bool IsActiveAt(DateTime utcNow) => RevokedAtUtc is null && ExpiresAtUtc > utcNow;

        // Records the replacement hash to preserve the rotation chain without storing secrets.
        public void Revoke(DateTime utcNow, string? replacedByTokenHash = null)
        {
            RevokedAtUtc = utcNow;
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }
}
