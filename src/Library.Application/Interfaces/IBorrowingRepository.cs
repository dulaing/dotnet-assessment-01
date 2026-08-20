using System;
using System.Collections.Generic;
using System.Text;

using Library.Domain.Entities;

namespace Library.Application.Interfaces
{
    public interface IBorrowingRepository
    {
        Task<List<Borrowing>> GetAllAsync(CancellationToken cancellationToken);
        Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<List<Borrowing>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken);
        Task<List<Borrowing>> GetActiveByMemberIdAsync(int memberId, CancellationToken cancellationToken); // get all active borrowings for a member
        Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken);
        void Update(Borrowing borrowing);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<bool> ExistsForBookAsync(int bookId, CancellationToken cancellationToken);
        Task<bool> ExistsForMemberAsync(int memberId, CancellationToken cancellationToken);

    }
}
