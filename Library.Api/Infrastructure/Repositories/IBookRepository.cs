using Library.Api.Domain.Entities;

namespace Library.Api.Infrastructure.Repositories;

// Defines persistence operations for book data.
public interface IBookRepository
{
    Task<List<Book>> GetAllAsync(CancellationToken cancellationToken);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken);
    Task AddAsync(Book book, CancellationToken cancellationToken);
    void Update(Book book);
    void Remove(Book book);
    Task<bool> HasBorrowingsAsync(int bookId, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
