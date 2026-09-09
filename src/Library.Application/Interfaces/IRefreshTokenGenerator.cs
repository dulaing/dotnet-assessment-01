using Library.Application.Common;

namespace Library.Application.Interfaces
{
    // Generates opaque refresh-token secrets and hashes presented token values.
    public interface IRefreshTokenGenerator
    {
        GeneratedRefreshToken Generate();
        string Hash(string token);
    }
}
