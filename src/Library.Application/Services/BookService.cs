using Library.Application.Contracts.Books;
using Library.Application.Interfaces;
using Library.Domain.Common;
using Library.Domain.Entities;

namespace Library.Application.Services
{
    // book crud, with the rules that keep isbn unique and copy counts honest
    public class BookService
    {
        private readonly IBookRepository _books;
        private readonly IBorrowingRepository _borrowings;

        public BookService(IBookRepository books, IBorrowingRepository borrowings)
        {
            _books = books;
            _borrowings = borrowings;
        }

        // get all books
        public async Task<List<BookResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            var books = await _books.GetAllAsync(cancellationToken);
            return books.Select(ToResponse).ToList();
        }

        // get one book
        public async Task<Result<BookResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var book = await _books.GetByIdAsync(id, cancellationToken);
            if (book is null)
            {
                return Result<BookResponse>.NotFound("book_not_found", $"Book {id} was not found.");
            }

            return Result<BookResponse>.Success(ToResponse(book));
        }

        // create a new book
        public async Task<Result<BookResponse>> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken)
        {
            var existing = await _books.GetByIsbnAsync(request.Isbn, cancellationToken);
            if (existing is not null)
            {
                return Result<BookResponse>.Conflict("isbn_already_exists", $"A book with ISBN {request.Isbn} already exists.");
            }

            // a brand new book has every copy sitting on the shelf
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                Isbn = request.Isbn,
                PublishedYear = request.PublishedYear,
                TotalCopies = request.TotalCopies,
                AvailableCopies = request.TotalCopies
            };

            await _books.AddAsync(book, cancellationToken);
            await _books.SaveChangesAsync(cancellationToken);

            return Result<BookResponse>.Success(ToResponse(book));
        }

        // update a book
        public async Task<Result<BookResponse>> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken)
        {
            var book = await _books.GetByIdAsync(id, cancellationToken);
            if (book is null)
            {
                return Result<BookResponse>.NotFound("book_not_found", $"Book {id} was not found.");
            }

            var isbnOwner = await _books.GetByIsbnAsync(request.Isbn, cancellationToken);
            if (isbnOwner is not null && isbnOwner.Id != id)
            {
                return Result<BookResponse>.Conflict("isbn_already_exists", $"A book with ISBN {request.Isbn} already exists.");
            }

            var onLoan = book.TotalCopies - book.AvailableCopies;
            if (request.TotalCopies < onLoan)
            {
                return Result<BookResponse>.Conflict("total_copies_below_borrowed", $"{onLoan} copies are currently on loan.");
            }

            book.Title = request.Title;
            book.Author = request.Author;
            book.Isbn = request.Isbn;
            book.PublishedYear = request.PublishedYear;
            book.TotalCopies = request.TotalCopies;
            book.AvailableCopies = request.TotalCopies - onLoan;

            _books.Update(book);
            await _books.SaveChangesAsync(cancellationToken);

            return Result<BookResponse>.Success(ToResponse(book));
        }

        // delete book
        public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var book = await _books.GetByIdAsync(id, cancellationToken);
            if (book is null)
            {
                return Result<bool>.NotFound("book_not_found", $"Book {id} was not found.");
            }

            if (await _borrowings.ExistsForBookAsync(id, cancellationToken))
            {
                return Result<bool>.Conflict("book_has_borrowings", $"Book {id} has borrowing history and cannot be deleted.");
            }

            _books.Remove(book);
            await _books.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }

        private static BookResponse ToResponse(Book book) => new(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
            book.PublishedYear,
            book.TotalCopies,
            book.AvailableCopies);
    }
}