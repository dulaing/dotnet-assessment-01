using Library.Api.Domain.Entities;

namespace Library.Api.Infrastructure.Repositories;

// Defines persistence operations for borrowing data.
public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken);
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Borrowing>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<Borrowing>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken);
    Task<int> CountActiveByMemberIdAsync(int memberId, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
