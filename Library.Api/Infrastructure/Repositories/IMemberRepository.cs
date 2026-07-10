using Library.Api.Domain.Entities;

namespace Library.Api.Infrastructure.Repositories;

// Defines persistence operations for member data.
public interface IMemberRepository
{
    Task<List<Member>> GetAllAsync(CancellationToken cancellationToken);
    Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Member member, CancellationToken cancellationToken);
    void Update(Member member);
    void Remove(Member member);
    Task<bool> HasBorrowingsAsync(int memberId, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
