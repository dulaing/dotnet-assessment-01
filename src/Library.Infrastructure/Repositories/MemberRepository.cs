using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryDbContext _db;

        public MemberRepository(LibraryDbContext db)
        {
            _db = db;
        }

        public Task<List<Member>> GetAllAsync(CancellationToken cancellationToken) =>
            _db.Members.AsNoTracking().ToListAsync(cancellationToken);

        public Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            _db.Members.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        public Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            _db.Members.FirstOrDefaultAsync(m => m.Email == email, cancellationToken);

        public async Task AddAsync(Member member, CancellationToken cancellationToken) =>
            await _db.Members.AddAsync(member, cancellationToken);

        public void Update(Member member) => _db.Members.Update(member);

        public void Remove(Member member) => _db.Members.Remove(member);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}