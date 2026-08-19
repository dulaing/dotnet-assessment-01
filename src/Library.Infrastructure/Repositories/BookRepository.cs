using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _db;

        // gets handed a database session
        public BookRepository(LibraryDbContext db)
        {
            _db = db;
        }

        // no tracking, because list results are read and thrown away
        
        // get every book
        public Task<List<Book>> GetAllAsync(CancellationToken cancellationToken) =>
            _db.Books.AsNoTracking().ToListAsync(cancellationToken);

        // get one book by ID
        public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            _db.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken) =>
            _db.Books.FirstOrDefaultAsync(b => b.Isbn == isbn, cancellationToken);

        // queue up a new book
        public async Task AddAsync(Book book, CancellationToken cancellationToken) =>
            await _db.Books.AddAsync(book, cancellationToken);

        // update and remove are no async
        public void Update(Book book) => _db.Books.Update(book);

        public void Remove(Book book) => _db.Books.Remove(book);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}