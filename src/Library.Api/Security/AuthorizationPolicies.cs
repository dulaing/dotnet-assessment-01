namespace Library.Api.Security
{
    // Keeps policy names consistent between registration and endpoint mappings.
    public static class AuthorizationPolicies
    {
        public const string AdminOnly = "AdminOnly";
    }
}
