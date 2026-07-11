namespace Library.Api.Domain.Enums;

// Matches the borrowing lifecycle states required by the assessment.
public enum BorrowingStatus
{
    Borrowed = 1,
    Returned = 2,
    Overdue = 3
}
