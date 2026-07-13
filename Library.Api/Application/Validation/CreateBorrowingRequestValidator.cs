using FluentValidation;
using Library.Api.Contracts.Borrowings;

namespace Library.Api.Application.Validation;

public sealed class CreateBorrowingRequestValidator : AbstractValidator<CreateBorrowingRequest>
{
    public CreateBorrowingRequestValidator()
    {
        RuleFor(request => request.BookId).GreaterThan(0);
        RuleFor(request => request.MemberId).GreaterThan(0);
    }
}
