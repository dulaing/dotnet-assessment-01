using FluentValidation;
using Library.Api.Contracts.Books;

namespace Library.Api.Application.Validation;

public sealed class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Author).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Isbn).NotEmpty().MaximumLength(20);
        RuleFor(request => request.TotalCopies).GreaterThan(0);
        RuleFor(request => request.PublishedYear)
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Year)
            .WithMessage("Published year cannot be in the future.");
    }
}
