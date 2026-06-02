using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty()
            .WithMessage("Email or username is required.")
            .MaximumLength(ValidationLimits.EmailMaxLength)
            .WithMessage($"Cannot exceed {ValidationLimits.EmailMaxLength} characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(ValidationLimits.PasswordMinLength)
            .WithMessage(ValidationMessages.PasswordTooShort)
            .Matches(ValidationPatterns.StrongPassword)
            .WithMessage(ValidationMessages.WeakPassword);

        RuleFor(x => x.DeviceId).NotEmpty().WithMessage("DeviceId is required.").MaximumLength(128);
    }
}
