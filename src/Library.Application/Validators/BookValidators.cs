using FluentValidation;
using Library.Application.Contracts.Books;

namespace Library.Application.Validators
{
    public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
    {
        public CreateBookRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Isbn).NotEmpty().MaximumLength(20);
            RuleFor(x => x.TotalCopies).GreaterThan(0);

            RuleFor(x => x.PublishedYear)
                .GreaterThan(0)
                .LessThanOrEqualTo(DateTime.UtcNow.Year)
                .WithMessage("Published year cannot be in the future.");
        }
    }

    public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
    {
        public UpdateBookRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Author).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Isbn).NotEmpty().MaximumLength(20);
            RuleFor(x => x.TotalCopies).GreaterThan(0);

            RuleFor(x => x.PublishedYear)
                .GreaterThan(0)
                .LessThanOrEqualTo(DateTime.UtcNow.Year)
                .WithMessage("Published year cannot be in the future.");
        }
    }
}