using FluentValidation;
using Library.Application.Contracts.Members;

namespace Library.Application.Validators
{
    public class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
    {
        public CreateMemberRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(x => x.PhoneNumber).MaximumLength(30);
        }
    }

    public class UpdateMemberRequestValidator : AbstractValidator<UpdateMemberRequest>
    {
        public UpdateMemberRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(x => x.PhoneNumber).MaximumLength(30);
        }
    }
}