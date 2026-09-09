namespace Library.Application.Common
{
    // Produces the canonical form used for email storage and comparison.
    public static class EmailNormalizer
    {
        public static string Normalize(string email) => email.Trim().ToLowerInvariant();
    }
}
