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
        private const int MaxActiveBorrowingsPerMember = 5;
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
            var member = await _members.GetByIdAsync(request.MemberId, cancellationToken);
            if (member is null)
            {
                return Result<BorrowingResponse>.NotFound("member_not_found", $"Member {request.MemberId} was not found.");
            }

            var book = await _books.GetByIdAsync(request.BookId, cancellationToken);
            if (book is null)
            {
                return Result<BorrowingResponse>.NotFound("book_not_found", $"Book {request.BookId} was not found.");
            }

            var canBorrow = member.EnsureCanBorrow();
            if (!canBorrow.IsSuccess)
            {
                return canBorrow.ToFailure<BorrowingResponse>();
            }

            var activeForMember = await _borrowings.GetActiveByMemberIdAsync(request.MemberId, cancellationToken);
            if (activeForMember.Count >= MaxActiveBorrowingsPerMember)
            {
                return Result<BorrowingResponse>.Conflict("borrowing_limit_exceeded", $"Member {request.MemberId} already holds {MaxActiveBorrowingsPerMember} books.");
            }

            // last check, because BorrowCopy mutates and a later failure would strand the decrement
            var borrowed = book.BorrowCopy();
            if (!borrowed.IsSuccess)
            {
                return borrowed.ToFailure<BorrowingResponse>();
            }

            var now = DateTime.UtcNow;
            var borrowing = new Borrowing
            {
                BookId = book.Id,
                MemberId = member.Id,
                BorrowedDate = now,
                DueDate = now.AddDays(LoanPeriodDays),
                Status = BorrowingStatus.Borrowed
            };

            await _borrowings.AddAsync(borrowing, cancellationToken);
            _books.Update(book);
            await _borrowings.SaveChangesAsync(cancellationToken);

            return Result<BorrowingResponse>.Success(new BorrowingResponse(
                borrowing.Id, borrowing.MemberId, borrowing.BookId,
                borrowing.BorrowedDate, borrowing.DueDate, borrowing.ReturnedDate,
                borrowing.Status.ToString()));
        }
    }
}
 