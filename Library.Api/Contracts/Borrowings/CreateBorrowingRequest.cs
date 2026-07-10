using System.ComponentModel.DataAnnotations;

namespace Library.Api.Contracts.Borrowings;

public sealed class CreateBorrowingRequest
{
    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(1, int.MaxValue)]
    public int MemberId { get; set; }
}
