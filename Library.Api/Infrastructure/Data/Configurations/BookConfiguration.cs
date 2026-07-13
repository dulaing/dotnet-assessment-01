using Library.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Api.Infrastructure.Data.Configurations;

// Owns everything about the Books table: shape, constraints, and seed rows.
public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(book => book.Id);
        builder.Property(book => book.Title).HasMaxLength(200);
        builder.Property(book => book.Author).HasMaxLength(150);
        builder.Property(book => book.Isbn).HasMaxLength(20);
        builder.HasIndex(book => book.Isbn).IsUnique();

        builder.HasData(
            new Book
            {
                Id = 1001,
                Title = "The Pragmatic Programmer",
                Author = "Andrew Hunt",
                Isbn = "9780201616224",
                PublishedYear = 1999,
                TotalCopies = 4,
                AvailableCopies = 4
            },
            new Book
            {
                Id = 1002,
                Title = "Clean Architecture",
                Author = "Robert C. Martin",
                Isbn = "9780134494166",
                PublishedYear = 2017,
                TotalCopies = 2,
                AvailableCopies = 2
            });
    }
}
