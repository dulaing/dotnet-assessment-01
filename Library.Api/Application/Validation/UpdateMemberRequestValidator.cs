using FluentValidation;
using Library.Api.Contracts.Members;

namespace Library.Api.Application.Validation;

public sealed class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequest>
{
    public UpdateMemberRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.PhoneNumber).MaximumLength(20);
    }
}
