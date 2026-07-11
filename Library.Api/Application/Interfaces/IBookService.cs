using Library.Api.Contracts.Books;

namespace Library.Api.Application.Interfaces;

// Defines the application-level use cases for book operations.
public interface IBookService
{
    Task<IReadOnlyList<BookResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<BookResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken);
    Task<BookResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
