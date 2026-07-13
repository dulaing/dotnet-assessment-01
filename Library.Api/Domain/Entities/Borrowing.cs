using Library.Api.Domain.Common;

namespace Library.Api.Domain.Entities;

public sealed class Borrowing : AuditableEntity<int>
{
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public Enums.BorrowingStatus Status { get; set; }

    // Marks a borrowing as returned exactly once.
    public void ReturnBook(DateTime returnedDateUtc)
    {
        if (ReturnedDate is not null || Status == Enums.BorrowingStatus.Returned)
        {
            throw new InvalidOperationException("Book has already been returned");
        }

        ReturnedDate = returnedDateUtc;
        Status = Enums.BorrowingStatus.Returned;
    }
}
