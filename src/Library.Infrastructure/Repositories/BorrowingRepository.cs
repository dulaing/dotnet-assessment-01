using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories
{
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly LibraryDbContext _db;

        public BorrowingRepository(LibraryDbContext db)
        {
            _db = db;
        }

        public Task<List<Borrowing>> GetAllAsync(CancellationToken cancellationToken) =>
            _db.Borrowings.AsNoTracking().ToListAsync(cancellationToken);

        public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            _db.Borrowings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        // full history for a member, newest first, unlike GetActiveByMemberIdAsync which only counts open loans
        public Task<List<Borrowing>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken) =>
            _db.Borrowings
                .AsNoTracking()
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.BorrowedDate)
                .ToListAsync(cancellationToken);

        // a loan is active while it has no returned date
        public Task<List<Borrowing>> GetActiveByMemberIdAsync(int memberId, CancellationToken cancellationToken) =>
            _db.Borrowings
                .AsNoTracking()
                .Where(b => b.MemberId == memberId && b.ReturnedDate == null)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken) =>
            await _db.Borrowings.AddAsync(borrowing, cancellationToken);

        public void Update(Borrowing borrowing) => _db.Borrowings.Update(borrowing);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            _db.SaveChangesAsync(cancellationToken);

        // deleting a book that has a loan history - return a clean 409
        // "does any borrowing row point at this book"
        // AnyAsync is a SELECT EXISTS, so Postgres stops at the first hit instead of counting everything.
        public Task<bool> ExistsForBookAsync(int bookId, CancellationToken cancellationToken) =>
            _db.Borrowings.AnyAsync(b => b.BookId == bookId, cancellationToken);

        public Task<bool> ExistsForMemberAsync(int memberId, CancellationToken cancellationToken) =>
            _db.Borrowings.AnyAsync(b => b.MemberId == memberId, cancellationToken);

    }
}