using System.Security.Cryptography;
using System.Text;
using Library.Application.Common;
using Library.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Library.Infrastructure.Security
{
    // Generates cryptographically random refresh tokens and deterministic SHA-256 hashes.
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public RefreshTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Returns a URL-safe secret while keeping its storage hash separate.
        public GeneratedRefreshToken Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays");
            return new GeneratedRefreshToken(token, Hash(token), DateTime.UtcNow.AddDays(expiryDays));
        }

        // Hashes the presented secret before any database lookup.
        public string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
