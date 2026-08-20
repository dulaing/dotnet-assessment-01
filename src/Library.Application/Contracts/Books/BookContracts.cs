namespace Library.Application.Contracts.Books
{
    public record CreateBookRequest(
        string Title, 
        string Author, 
        string Isbn, 
        int PublishedYear, 
        int TotalCopies
    );

    public record BookResponse(
        int Id, 
        string Title, 
        string Author, 
        string Isbn, 
        int PublishedYear, 
        int TotalCopies, 
        int AvailableCopies
    );

    public record UpdateBookRequest(
        string Title, 
        string Author, 
        string Isbn, 
        int PublishedYear, 
        int TotalCopies
    );
}