namespace Library.Api.Contracts.Borrowings;

public sealed class CreateBorrowingRequest
{
    public int BookId { get; set; }
    public int MemberId { get; set; }
}
