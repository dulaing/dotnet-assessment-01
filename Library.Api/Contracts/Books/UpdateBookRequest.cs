using System.ComponentModel.DataAnnotations;

namespace Library.Api.Contracts.Books;

public sealed class UpdateBookRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string Isbn { get; set; } = string.Empty;

    public int PublishedYear { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableCopies { get; set; }
}
