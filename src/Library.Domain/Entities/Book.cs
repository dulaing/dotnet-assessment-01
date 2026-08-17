using Library.Domain.Common;

namespace Library.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int TotalCopies {  get; set; }
        public int AvailableCopies { get; set; }

        public Result<Book> BorrowCopy()
        {
            if (AvailableCopies <= 0)
            {
                return Result<Book>.Conflict("book_unavailable", "No copies available to borrow.");
            }

            AvailableCopies -= 1;
            return Result<Book>.Success(this);
        }

        public Result<Book> ReturnCopy()
        {
            if (AvailableCopies >= TotalCopies)
            {
                return Result<Book>.Conflict("book_already_returned", "All copies are already available");
            }

            AvailableCopies += 1;
            return Result<Book>.Success(this);
        }
    }
}

