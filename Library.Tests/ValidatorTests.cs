using FluentValidation;
using Library.Api.Application.Validation;
using Library.Api.Contracts.Books;
using Library.Api.Contracts.Members;

namespace Library.Tests;

public sealed class ValidatorTests
{
    [Fact]
    public void CreateBook_WithValidRequest_Passes()
    {
        var result = new CreateBookRequestValidator().Validate(new CreateBookRequest
        {
            Title = "Refactoring",
            Author = "Martin Fowler",
            Isbn = "9780201485677",
            PublishedYear = 1999,
            TotalCopies = 3
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateBook_WithEmptyTitle_FailsOnTitle()
    {
        var result = new CreateBookRequestValidator().Validate(new CreateBookRequest
        {
            Title = "",
            Author = "Author",
            Isbn = "isbn",
            PublishedYear = 2000,
            TotalCopies = 1
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Title");
    }

    [Fact]
    public void CreateBook_WithFuturePublishedYear_FailsOnPublishedYear()
    {
        var result = new CreateBookRequestValidator().Validate(new CreateBookRequest
        {
            Title = "Title",
            Author = "Author",
            Isbn = "isbn",
            PublishedYear = DateTime.UtcNow.Year + 1,
            TotalCopies = 1
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "PublishedYear");
    }

    [Fact]
    public void CreateBook_WithZeroTotalCopies_FailsOnTotalCopies()
    {
        var result = new CreateBookRequestValidator().Validate(new CreateBookRequest
        {
            Title = "Title",
            Author = "Author",
            Isbn = "isbn",
            PublishedYear = 2000,
            TotalCopies = 0
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TotalCopies");
    }

    [Fact]
    public void UpdateBook_WithAvailableGreaterThanTotal_FailsOnAvailableCopies()
    {
        var result = new UpdateBookRequestValidator().Validate(new UpdateBookRequest
        {
            Title = "Title",
            Author = "Author",
            Isbn = "isbn",
            PublishedYear = 2000,
            TotalCopies = 2,
            AvailableCopies = 3
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "AvailableCopies");
    }

    [Fact]
    public void CreateMember_WithInvalidEmail_FailsOnEmail()
    {
        var result = new CreateMemberRequestValidator().Validate(new CreateMemberRequest
        {
            FullName = "Alan Turing",
            Email = "not-an-email"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
    }
}
