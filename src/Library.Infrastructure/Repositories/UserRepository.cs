using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LibraryDbContext _db;

        public UserRepository(LibraryDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken) =>
            await _db.Users.AddAsync(user, cancellationToken);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}