namespace Library.Application.Contracts.Borrowings
{
    public record CreateBorrowingRequest (
        int MemberId, 
        int BookId
    );

    public record BorrowingResponse(
        int Id, 
        int MemberId, 
        int BookId, 
        DateTime BorrowedDate, 
        DateTime DueDate, 
        DateTime? ReturnedDate, 
        string Status
    );
}