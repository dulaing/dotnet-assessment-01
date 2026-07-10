using Library.Api.Domain.Entities;
using Library.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Infrastructure.Repositories;

// Will contain EF-backed book queries and commands.
public sealed class BookRepository(LibraryDbContext dbContext) : IBookRepository
{
    public Task<List<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Books
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Books.FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken)
    {
        return dbContext.Books.FirstOrDefaultAsync(book => book.Isbn == isbn, cancellationToken);
    }

    public Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        return dbContext.Books.AddAsync(book, cancellationToken).AsTask();
    }

    public void Update(Book book)
    {
        dbContext.Books.Update(book);
    }

    public void Remove(Book book)
    {
        dbContext.Books.Remove(book);
    }

    public Task<bool> HasBorrowingsAsync(int bookId, CancellationToken cancellationToken)
    {
        return dbContext.Borrowings.AnyAsync(borrowing => borrowing.BookId == bookId, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
