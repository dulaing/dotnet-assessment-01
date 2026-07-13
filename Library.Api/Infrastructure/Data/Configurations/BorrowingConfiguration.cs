using Library.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Api.Infrastructure.Data.Configurations;

// Owns the Borrowings table and its restricted foreign keys to books and members.
public sealed class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.HasKey(borrowing => borrowing.Id);

        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(borrowing => borrowing.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(borrowing => borrowing.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
