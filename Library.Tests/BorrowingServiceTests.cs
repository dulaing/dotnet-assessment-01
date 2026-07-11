using Library.Api.Application.Exceptions;
using Library.Api.Application.Services;
using Library.Api.Contracts.Borrowings;
using Library.Api.Domain.Entities;
using Library.Api.Domain.Enums;
using Library.Api.Infrastructure.Repositories;

namespace Library.Tests;

public sealed class BorrowingServiceTests
{
    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenNoCopiesAreAvailable()
    {
        var service = CreateService(
            books:
            [
                new Book
                {
                    Id = 1,
                    Title = "Unavailable",
                    Author = "Author",
                    Isbn = "isbn-1",
                    PublishedYear = 2001,
                    TotalCopies = 1,
                    AvailableCopies = 0
                }
            ],
            members:
            [
                new Member
                {
                    Id = 1,
                    FullName = "Active Member",
                    Email = "active@example.com",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true
                }
            ]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateBorrowingRequest { BookId = 1, MemberId = 1 }, CancellationToken.None));

        Assert.Equal("Book is unavailable", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenMemberIsInactive()
    {
        var service = CreateService(
            books:
            [
                new Book
                {
                    Id = 1,
                    Title = "Available",
                    Author = "Author",
                    Isbn = "isbn-1",
                    PublishedYear = 2001,
                    TotalCopies = 1,
                    AvailableCopies = 1
                }
            ],
            members:
            [
                new Member
                {
                    Id = 1,
                    FullName = "Inactive Member",
                    Email = "inactive@example.com",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = false
                }
            ]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateBorrowingRequest { BookId = 1, MemberId = 1 }, CancellationToken.None));

        Assert.Equal("Member is inactive", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenMemberAlreadyHasThreeActiveBorrowings()
    {
        var service = CreateService(
            books:
            [
                new Book
                {
                    Id = 1,
                    Title = "Available",
                    Author = "Author",
                    Isbn = "isbn-1",
                    PublishedYear = 2001,
                    TotalCopies = 4,
                    AvailableCopies = 4
                }
            ],
            members:
            [
                new Member
                {
                    Id = 1,
                    FullName = "Busy Member",
                    Email = "busy@example.com",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true
                }
            ],
            borrowings:
            [
                CreateBorrowing(1, 1, BorrowingStatus.Borrowed),
                CreateBorrowing(2, 1, BorrowingStatus.Borrowed),
                CreateBorrowing(3, 1, BorrowingStatus.Borrowed)
            ]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateBorrowingRequest { BookId = 1, MemberId = 1 }, CancellationToken.None));

        Assert.Equal("Member borrowing limit exceeded", exception.Message);
    }

    [Fact]
    public async Task ReturnAsync_IncreasesAvailableCopies()
    {
        var book = new Book
        {
            Id = 1,
            Title = "Borrowed",
            Author = "Author",
            Isbn = "isbn-1",
            PublishedYear = 2001,
            TotalCopies = 2,
            AvailableCopies = 1
        };
        var service = CreateService(
            books: [book],
            members:
            [
                new Member
                {
                    Id = 1,
                    FullName = "Member",
                    Email = "member@example.com",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true
                }
            ],
            borrowings: [CreateBorrowing(1, 1, BorrowingStatus.Borrowed, bookId: 1)]);

        await service.ReturnAsync(1, CancellationToken.None);

        Assert.Equal(2, book.AvailableCopies);
    }

    [Fact]
    public async Task ReturnAsync_ThrowsConflict_WhenBookIsReturnedTwice()
    {
        var service = CreateService(
            books:
            [
                new Book
                {
                    Id = 1,
                    Title = "Returned",
                    Author = "Author",
                    Isbn = "isbn-1",
                    PublishedYear = 2001,
                    TotalCopies = 2,
                    AvailableCopies = 2
                }
            ],
            members:
            [
                new Member
                {
                    Id = 1,
                    FullName = "Member",
                    Email = "member@example.com",
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true
                }
            ],
            borrowings:
            [
                CreateBorrowing(1, 1, BorrowingStatus.Returned, returnedDate: DateTime.UtcNow, bookId: 1)
            ]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ReturnAsync(1, CancellationToken.None));

        Assert.Equal("Book has already been returned", exception.Message);
    }

    private static BorrowingService CreateService(
        IEnumerable<Book>? books = null,
        IEnumerable<Member>? members = null,
        IEnumerable<Borrowing>? borrowings = null)
    {
        return new BorrowingService(
            new FakeBorrowingRepository(borrowings),
            new FakeBookRepository(books),
            new FakeMemberRepository(members));
    }

    private static Borrowing CreateBorrowing(
        int id,
        int memberId,
        BorrowingStatus status,
        int bookId = 99,
        DateTime? returnedDate = null)
    {
        return new Borrowing
        {
            Id = id,
            BookId = bookId,
            MemberId = memberId,
            BorrowedDate = DateTime.UtcNow.AddDays(-2),
            DueDate = DateTime.UtcNow.AddDays(12),
            ReturnedDate = returnedDate,
            Status = status
        };
    }

    private sealed class FakeBookRepository(IEnumerable<Book>? books) : IBookRepository
    {
        private readonly List<Book> _books = books?.ToList() ?? [];

        public Task<List<Book>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_books.ToList());
        }

        public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_books.FirstOrDefault(book => book.Id == id));
        }

        public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken)
        {
            return Task.FromResult(_books.FirstOrDefault(book => book.Isbn == isbn));
        }

        public Task AddAsync(Book book, CancellationToken cancellationToken)
        {
            _books.Add(book);
            return Task.CompletedTask;
        }

        public void Update(Book book)
        {
        }

        public void Remove(Book book)
        {
            _books.Remove(book);
        }

        public Task<bool> HasBorrowingsAsync(int bookId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    private sealed class FakeMemberRepository(IEnumerable<Member>? members) : IMemberRepository
    {
        private readonly List<Member> _members = members?.ToList() ?? [];

        public Task<List<Member>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_members.ToList());
        }

        public Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_members.FirstOrDefault(member => member.Id == id));
        }

        public Task<Member?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(_members.FirstOrDefault(member => member.Email == email));
        }

        public Task AddAsync(Member member, CancellationToken cancellationToken)
        {
            _members.Add(member);
            return Task.CompletedTask;
        }

        public void Update(Member member)
        {
        }

        public void Remove(Member member)
        {
            _members.Remove(member);
        }

        public Task<bool> HasBorrowingsAsync(int memberId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    private sealed class FakeBorrowingRepository(IEnumerable<Borrowing>? borrowings) : IBorrowingRepository
    {
        private readonly List<Borrowing> _borrowings = borrowings?.ToList() ?? [];

        public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken)
        {
            borrowing.Id = _borrowings.Count == 0 ? 1 : _borrowings.Max(item => item.Id) + 1;
            _borrowings.Add(borrowing);
            return Task.CompletedTask;
        }

        public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_borrowings.FirstOrDefault(borrowing => borrowing.Id == id));
        }

        public Task<List<Borrowing>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_borrowings.ToList());
        }

        public Task<List<Borrowing>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_borrowings.Where(borrowing => borrowing.MemberId == memberId).ToList());
        }

        public Task<int> CountActiveByMemberIdAsync(int memberId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_borrowings.Count(
                borrowing => borrowing.MemberId == memberId && borrowing.Status == BorrowingStatus.Borrowed));
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
