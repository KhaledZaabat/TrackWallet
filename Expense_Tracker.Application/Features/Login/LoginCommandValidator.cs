using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(ValidationLimits.PasswordMinLength)
            .WithMessage(ValidationMessages.PasswordTooShort)
            .Matches(ValidationPatterns.StrongPassword)
            .WithMessage(ValidationMessages.WeakPassword);

        RuleFor(x => x.DeviceId)
          .NotEmpty()
          .WithMessage("DeviceId is required.")
          .MaximumLength(128);
    }
}
