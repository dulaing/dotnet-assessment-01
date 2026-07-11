namespace Library.Api.Domain.Entities;

public sealed class Borrowing
{
    private DateTime _borrowedDate;
    private DateTime _dueDate;
    private DateTime? _returnedDate;

    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime BorrowedDate
    {
        get => _borrowedDate;
        set => _borrowedDate = NormalizeUtc(value);
    }

    public DateTime DueDate
    {
        get => _dueDate;
        set => _dueDate = NormalizeUtc(value);
    }

    public DateTime? ReturnedDate
    {
        get => _returnedDate;
        set => _returnedDate = value is null ? null : NormalizeUtc(value.Value);
    }

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

    private static DateTime NormalizeUtc(DateTime value)
    {
        if (value == default)
        {
            return value;
        }

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value.ToUniversalTime()
        };
    }
}
