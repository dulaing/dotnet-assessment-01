namespace Library.Api.Contracts.Borrowings;

public sealed class BorrowingResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
