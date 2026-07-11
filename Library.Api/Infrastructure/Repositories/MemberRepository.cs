using Library.Api.Domain.Entities;
using Library.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Infrastructure.Repositories;

// Will contain EF-backed member queries and commands.
public sealed class MemberRepository(LibraryDbContext dbContext) : IMemberRepository
{
    public Task<List<Member>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Members
            .AsNoTracking()
            .OrderBy(member => member.FullName)
            .ToListAsync(cancellationToken);
    }

    public Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Members.FirstOrDefaultAsync(member => member.Id == id, cancellationToken);
    }

    public Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Members.FirstOrDefaultAsync(member => member.Email == email, cancellationToken);
    }

    public Task AddAsync(Member member, CancellationToken cancellationToken)
    {
        return dbContext.Members.AddAsync(member, cancellationToken).AsTask();
    }

    public void Update(Member member)
    {
        dbContext.Members.Update(member);
    }

    public void Remove(Member member)
    {
        dbContext.Members.Remove(member);
    }

    public Task<bool> HasBorrowingsAsync(int memberId, CancellationToken cancellationToken)
    {
        return dbContext.Borrowings.AnyAsync(borrowing => borrowing.MemberId == memberId, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
