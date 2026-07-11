namespace Library.Api.Domain.Entities;

public sealed class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }

    // Keeps copy counts consistent when a book is borrowed.
    public void BorrowCopy()
    {
        if (AvailableCopies <= 0)
        {
            throw new InvalidOperationException("Book is unavailable");
        }

        AvailableCopies -= 1;
    }

    // Restores availability when a borrowed copy comes back.
    public void ReturnCopy()
    {
        if (AvailableCopies >= TotalCopies)
        {
            throw new InvalidOperationException("All book copies are already available");
        }

        AvailableCopies += 1;
    }
}
