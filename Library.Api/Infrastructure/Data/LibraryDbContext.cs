using Library.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Infrastructure.Data;

// Central EF Core unit of work for the library schema.
public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // These mappings enforce the database constraints called out in the assessment.
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Title).HasMaxLength(200);
            entity.Property(book => book.Author).HasMaxLength(150);
            entity.Property(book => book.Isbn).HasMaxLength(20);
            entity.HasIndex(book => book.Isbn).IsUnique();
            entity.HasData(
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
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(member => member.Id);
            entity.Property(member => member.FullName).HasMaxLength(150);
            entity.Property(member => member.Email).HasMaxLength(256);
            entity.Property(member => member.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(member => member.Email).IsUnique();
            entity.HasData(
                new Member
                {
                    Id = 2001,
                    FullName = "Ada Lovelace",
                    Email = "ada.seed@example.com",
                    PhoneNumber = "555-1001",
                    RegisteredDate = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new Member
                {
                    Id = 2002,
                    FullName = "Grace Hopper",
                    Email = "grace.seed@example.com",
                    PhoneNumber = "555-1002",
                    RegisteredDate = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                });
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.HasKey(borrowing => borrowing.Id);
            entity.HasOne<Book>()
                .WithMany()
                .HasForeignKey(borrowing => borrowing.BookId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Member>()
                .WithMany()
                .HasForeignKey(borrowing => borrowing.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
