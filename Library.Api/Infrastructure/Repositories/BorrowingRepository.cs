using Library.Api.Domain.Entities;
using Library.Api.Domain.Enums;
using Library.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Infrastructure.Repositories;

// Will contain EF-backed borrowing queries and commands.
public sealed class BorrowingRepository(LibraryDbContext dbContext) : IBorrowingRepository
{
    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken)
    {
        return dbContext.Borrowings.AddAsync(borrowing, cancellationToken).AsTask();
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Borrowings.FirstOrDefaultAsync(borrowing => borrowing.Id == id, cancellationToken);
    }

    public Task<List<Borrowing>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Borrowings
            .AsNoTracking()
            .OrderByDescending(borrowing => borrowing.BorrowedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Borrowing>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken)
    {
        return dbContext.Borrowings
            .AsNoTracking()
            .Where(borrowing => borrowing.MemberId == memberId)
            .OrderByDescending(borrowing => borrowing.BorrowedDate)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveByMemberIdAsync(int memberId, CancellationToken cancellationToken)
    {
        return dbContext.Borrowings.CountAsync(
            borrowing => borrowing.MemberId == memberId && borrowing.Status == BorrowingStatus.Borrowed,
            cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
