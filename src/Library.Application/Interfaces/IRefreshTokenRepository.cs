using Library.Domain.Entities;

namespace Library.Application.Interfaces
{
    // Provides persistence operations required for refresh-token rotation.
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
