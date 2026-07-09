namespace Library.Api.Domain.Entities;

public sealed class Borrowing
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public Enums.BorrowingStatus Status { get; set; }
}
