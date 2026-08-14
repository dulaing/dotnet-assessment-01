using System;
using System.Collections.Generic;
using System.Text;

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

        public void ReturnBook(DateTime returnedDateUtc)
        {
            if (ReturnedDate is not null || Status == BorrowingStatus.Returned)
            {
                throw new InvalidOperationException("Book has alraedy been returned");
            }

            ReturnedDate = returnedDateUtc;
            Status = BorrowingStatus.Returned;
        }
    }
}
