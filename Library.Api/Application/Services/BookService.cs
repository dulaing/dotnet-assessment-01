using Library.Api.Application.Exceptions;
using Library.Api.Application.Interfaces;
using Library.Api.Contracts.Books;
using Library.Api.Domain.Entities;
using Library.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Application.Services;

// Holds book-specific business rules outside the HTTP layer.
public sealed class BookService(IBookRepository bookRepository) : IBookService
{
    public async Task<IReadOnlyList<BookResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var books = await bookRepository.GetAllAsync(cancellationToken);

        return books.Select(MapResponse).ToArray();
    }

    public async Task<BookResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Book not found");

        return MapResponse(book);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var normalizedIsbn = request.Isbn.Trim();
        await EnsureIsbnIsUniqueAsync(normalizedIsbn, null, cancellationToken);

        // New books start with every copy available until borrowings exist.
        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = normalizedIsbn,
            PublishedYear = request.PublishedYear,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies
        };

        await bookRepository.AddAsync(book, cancellationToken);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(book);
    }

    public async Task<BookResponse> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Book not found");

        var normalizedIsbn = request.Isbn.Trim();
        await EnsureIsbnIsUniqueAsync(normalizedIsbn, id, cancellationToken);

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Isbn = normalizedIsbn;
        book.PublishedYear = request.PublishedYear;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.AvailableCopies;

        bookRepository.Update(book);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(book);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Book not found");

        if (await bookRepository.HasBorrowingsAsync(id, cancellationToken))
        {
            throw new ConflictException("Book cannot be deleted because borrowing history exists");
        }

        bookRepository.Remove(book);

        try
        {
            await bookRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Book cannot be deleted because borrowing history exists");
        }
    }

    private async Task EnsureIsbnIsUniqueAsync(string isbn, int? currentBookId, CancellationToken cancellationToken)
    {
        var existingBook = await bookRepository.GetByIsbnAsync(isbn, cancellationToken);

        if (existingBook is not null && existingBook.Id != currentBookId)
        {
            throw new ConflictException("A book with the same ISBN already exists");
        }
    }

    private static BookResponse MapResponse(Book book)
    {
        return new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            PublishedYear = book.PublishedYear,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies
        };
    }
}
