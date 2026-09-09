using FluentValidation;
using Library.Application.Contracts.Users;
using Library.Domain.Enums;

namespace Library.Application.Validators
{
    // Validates account credentials and role input before account creation.
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(128)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
            RuleFor(x => x.Role)
                .Must(role => Enum.TryParse<UserRole>(role, true, out _))
                .WithMessage("Role must be Admin or Member.");
            RuleFor(x => x.MemberId).GreaterThan(0).When(x => x.MemberId is not null);
        }
    }
}
