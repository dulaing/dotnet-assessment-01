using Library.Application.Contracts.Borrowings;
using Library.Application.Interfaces;
using Library.Domain.Common;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services
{
    // orchestrates the borrow flow across member, book, and borrowing
    public class BorrowingService
    {
        private const int MaxActiveBorrowingsPerMember = 3;
        private const int LoanPeriodDays = 14;

        private readonly IBookRepository _books;
        private readonly IMemberRepository _members;
        private readonly IBorrowingRepository _borrowings;

        public BorrowingService(IBookRepository books, IMemberRepository members, IBorrowingRepository borrowings)
        {
            _books = books;
            _members = members;
            _borrowings = borrowings;
        }

        public async Task<Result<BorrowingResponse>> BorrowAsync(CreateBorrowingRequest request, CancellationToken cancellationToken)
        {
            // is the member real?
            var member = await _members.GetByIdAsync(request.MemberId, cancellationToken);
            if (member is null)
            {
                return Result<BorrowingResponse>.NotFound("member_not_found", $"Member {request.MemberId} was not found.");
            }

            // is the book real?
            var book = await _books.GetByIdAsync(request.BookId, cancellationToken);
            if (book is null)
            {
                return Result<BorrowingResponse>.NotFound("book_not_found", $"Book {request.BookId} was not found.");
            }

            // can  the member actually borrow?
            var canBorrow = member.EnsureCanBorrow();
            if (!canBorrow.IsSuccess)
            {
                return canBorrow.ToFailure<BorrowingResponse>();
            }

            // are they hogging too many books?
            var activeForMember = await _borrowings.GetActiveByMemberIdAsync(request.MemberId, cancellationToken);
            if (activeForMember.Count >= MaxActiveBorrowingsPerMember)
            {
                return Result<BorrowingResponse>.Conflict("borrowing_limit_exceeded", $"Member {request.MemberId} already holds {MaxActiveBorrowingsPerMember} books.");
            }

            // is the copy actually on the shelf?
            // last check, because BorrowCopy mutates and a later failure would strand the decrement
            var borrowed = book.BorrowCopy();
            if (!borrowed.IsSuccess)
            {
                return borrowed.ToFailure<BorrowingResponse>();
            }

            // write in the ledger: who took what, today's date, due in 14 days
            var now = DateTime.UtcNow;
            var borrowing = new Borrowing
            {
                BookId = book.Id,
                MemberId = member.Id,
                BorrowedDate = now,
                DueDate = now.AddDays(LoanPeriodDays),
                Status = BorrowingStatus.Borrowed
            };

            // save
            await _borrowings.AddAsync(borrowing, cancellationToken);
            _books.Update(book);
            await _borrowings.SaveChangesAsync(cancellationToken);

            return Result<BorrowingResponse>.Success(ToResponse(borrowing));
        }

        // someone returns the book, same desk, reverse flow
        public async Task<Result<BorrowingResponse>> ReturnAsync(int borrowingId, CancellationToken cancellationToken)
        {
            // which loan is this? no entry, send them away
            var borrowing = await _borrowings.GetByIdAsync(borrowingId, cancellationToken);
            if (borrowing is null)
            {
                return Result<BorrowingResponse>.NotFound("borrowing_not_found", $"Borrowing {borrowingId} was not found.");
            }

            // which book was it
            var book = await _books.GetByIdAsync(borrowing.BookId, cancellationToken);
            if (book is null)
            {
                return Result<BorrowingResponse>.NotFound("book_not_found", $"Book {borrowing.BookId} was not found.");
            }

            // close the ledger entry
            // borrowing first, because a double return is the only rejection a caller can actually trigger
            var returned = borrowing.ReturnBook(DateTime.UtcNow);
            if (!returned.IsSuccess)
            {
                return returned.ToFailure<BorrowingResponse>();
            }

            // put the copy back on the shelf, add 1 to the availble count
            var restored = book.ReturnCopy();
            if (!restored.IsSuccess)
            {
                return restored.ToFailure<BorrowingResponse>();
            }

            _borrowings.Update(borrowing);
            _books.Update(book);
            await _borrowings.SaveChangesAsync(cancellationToken);

            return Result<BorrowingResponse>.Success(ToResponse(borrowing));
        }

        private static BorrowingResponse ToResponse(Borrowing borrowing) => new(
            borrowing.Id,
            borrowing.MemberId,
            borrowing.BookId,
            borrowing.BorrowedDate,
            borrowing.DueDate,
            borrowing.ReturnedDate,
            borrowing.Status.ToString());
    }
}
 