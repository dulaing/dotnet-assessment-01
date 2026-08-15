using System;
using System.Collections.Generic;
using System.Text;

using Library.Domain.Entities;

namespace Library.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllAsync(CancellationToken cancellationToken);
        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken);
        Task AddAsync(Book book, CancellationToken cancellationToken);
        void Update(Book book);
        void Remove(Book book);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken); 

    }
}
