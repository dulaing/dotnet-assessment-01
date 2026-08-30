using Library.Application.Interfaces;
using Library.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Library.Infrastructure.Security
{
    // wraps the asp.net hasher so the application layer never references the identity package
    public class IdentityPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<User> _inner = new();

        // the user argument is ignored by the default implementation, so null is safe here
        public string Hash(string password) => _inner.HashPassword(null!, password);

        public bool Verify(string password, string hash) =>
            _inner.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
    }
}