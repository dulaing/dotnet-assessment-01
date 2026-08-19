using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations
{
    public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
    {
        public void Configure(EntityTypeBuilder<Borrowing> builder)
        {
            builder.ToTable("borrowings");
            builder.HasKey(b => b.Id);

            // status is stored as a number
            builder.Property(b => b.Status).IsRequired();

            // HasOne means each borrowing has one book
            // also cannot delete a book that has loan history - it'll delete the borrowings too
            builder.HasOne<Book>()
                .WithMany()
                .HasForeignKey(b => b.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Member>()
                .WithMany()
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // covers the active-loans-per-member lookup the borrowing limit depends on
            builder.HasIndex(b => new { b.MemberId, b.ReturnedDate });
        }
    }
}