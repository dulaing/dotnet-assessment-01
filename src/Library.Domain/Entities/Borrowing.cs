using System;
using System.Collections.Generic;
using System.Text;

using Library.Domain.Common;
using Library.Domain.Enums;

namespace Library.Domain.Entities
{
    public class Borrowing
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public BorrowingStatus Status { get; set; }

        public Result<Borrowing> ReturnBook(DateTime returnedDateUtc)
        {
            if (ReturnedDate is not null || Status == BorrowingStatus.Returned)
            {
                return Result<Borrowing>.Conflict("book_already_returned", "This borrowing was already returned.");
            }

            ReturnedDate = returnedDateUtc;
            Status = BorrowingStatus.Returned;
            return Result<Borrowing>.Success(this);
        }
    }
}
