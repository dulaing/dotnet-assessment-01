using Library.Application.Common;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence
{
    public static class DatabaseSeeder
    {
        private const int SeedLoanPeriodDays = 14;

        // single entry point, so Program.cs does not need to know what gets seeded
        public static async Task SeedAsync(this IServiceProvider services)
        {
            // repositories are scoped, so startup code has to open its own scope to resolve them
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var db = provider.GetRequiredService<LibraryDbContext>();
            await db.Database.MigrateAsync();

            var configuration = provider.GetRequiredService<IConfiguration>();

            await SeedAdminAsync(provider, configuration);

            var member = await SeedMemberAsync(provider, configuration);
            if (member is not null)
            {
                await SeedCatalogueAsync(provider, member);
            }
        }

        // the first admin, without which nobody could ever create anything
        private static async Task SeedAdminAsync(IServiceProvider provider, IConfiguration configuration)
        {
            var email = configuration["Seed:AdminEmail"];
            var password = configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            email = EmailNormalizer.Normalize(email);
            var users = provider.GetRequiredService<IUserRepository>();

            if (await users.GetByEmailAsync(email, CancellationToken.None) is not null)
            {
                return;
            }

            var hasher = provider.GetRequiredService<IPasswordHasher>();

            await users.AddAsync(new User
            {
                Email = email,
                PasswordHash = hasher.Hash(password),
                Role = UserRole.Admin,
                MemberId = null
            }, CancellationToken.None);

            await users.SaveChangesAsync(CancellationToken.None);
        }

        // a normal member plus the login that maps to it, so ownership rules can be exercised
        private static async Task<Member?> SeedMemberAsync(IServiceProvider provider, IConfiguration configuration)
        {
            var email = configuration["Seed:MemberEmail"];
            var password = configuration["Seed:MemberPassword"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            email = EmailNormalizer.Normalize(email);

            var members = provider.GetRequiredService<IMemberRepository>();
            var member = await members.GetByEmailAsync(email, CancellationToken.None);

            if (member is null)
            {
                member = new Member
                {
                    FullName = "Jane Doe",
                    Email = email,
                    PhoneNumber = "+94770000000",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true
                };

                await members.AddAsync(member, CancellationToken.None);
                await members.SaveChangesAsync(CancellationToken.None);
            }

            var users = provider.GetRequiredService<IUserRepository>();

            if (await users.GetByEmailAsync(email, CancellationToken.None) is null)
            {
                var hasher = provider.GetRequiredService<IPasswordHasher>();

                // MemberId is what becomes the member_id claim, so every ownership check resolves here
                await users.AddAsync(new User
                {
                    Email = email,
                    PasswordHash = hasher.Hash(password),
                    Role = UserRole.Member,
                    MemberId = member.Id
                }, CancellationToken.None);

                await users.SaveChangesAsync(CancellationToken.None);
            }

            return member;
        }

        // sample catalogue and loans, only on an empty shelf so restarts do not pile up duplicates
        private static async Task SeedCatalogueAsync(IServiceProvider provider, Member member)
        {
            var books = provider.GetRequiredService<IBookRepository>();

            if ((await books.GetAllAsync(CancellationToken.None)).Count > 0)
            {
                return;
            }

            var catalogue = new List<Book>
            {
                NewBook("Clean Code", "Robert C. Martin", "9780132350884", 2008, 3),
                NewBook("The Pragmatic Programmer", "Andrew Hunt", "9780201616224", 1999, 2),
                NewBook("Design Patterns", "Erich Gamma", "9780201633610", 1994, 2),
                NewBook("Refactoring", "Martin Fowler", "9780201485677", 1999, 1),
                NewBook("Domain-Driven Design", "Eric Evans", "9780321125217", 2003, 2),
                NewBook("Dune", "Frank Herbert", "9780441013593", 1965, 4),
                NewBook("The Hobbit", "J. R. R. Tolkien", "9780547928227", 1937, 5)
            };

            foreach (var book in catalogue)
            {
                await books.AddAsync(book, CancellationToken.None);
            }

            await books.SaveChangesAsync(CancellationToken.None);

            var borrowings = provider.GetRequiredService<IBorrowingRepository>();
            var now = DateTime.UtcNow;

            // one healthy loan, and one already past its due date so the overdue scan has something to find
            AddBorrowing(borrowings, books, catalogue[0], member, now.AddDays(-3));
            AddBorrowing(borrowings, books, catalogue[5], member, now.AddDays(-30));

            await borrowings.SaveChangesAsync(CancellationToken.None);
        }

        private static Book NewBook(string title, string author, string isbn, int publishedYear, int totalCopies) => new()
        {
            Title = title,
            Author = author,
            Isbn = isbn,
            PublishedYear = publishedYear,
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies
        };

        private static void AddBorrowing(
            IBorrowingRepository borrowings,
            IBookRepository books,
            Book book,
            Member member,
            DateTime borrowedAtUtc)
        {
            // going through the domain method keeps AvailableCopies honest instead of hand setting it
            var borrowed = book.BorrowCopy();
            if (!borrowed.IsSuccess)
            {
                throw new InvalidOperationException($"Seed data is inconsistent: {borrowed.Error!.Message}");
            }

            books.Update(book);

            borrowings.AddAsync(new Borrowing
            {
                BookId = book.Id,
                MemberId = member.Id,
                BorrowedDate = borrowedAtUtc,
                DueDate = borrowedAtUtc.AddDays(SeedLoanPeriodDays),
                Status = BorrowingStatus.Borrowed
            }, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}