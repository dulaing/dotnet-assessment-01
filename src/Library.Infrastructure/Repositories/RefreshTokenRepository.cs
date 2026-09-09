using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories
{
    // Persists refresh-token hashes through the shared EF Core unit of work.
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly LibraryDbContext _db;

        public RefreshTokenRepository(LibraryDbContext db)
        {
            _db = db;
        }

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
            _db.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
            await _db.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}
