using Library.Domain.Entities;

namespace Library.Application.Interfaces
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetAllAsync(CancellationToken cancellationToken);
        Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task AddAsync(Member member, CancellationToken cancellationToken);
        void Update(Member member);
        void Remove(Member member);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken); 

    }
}
