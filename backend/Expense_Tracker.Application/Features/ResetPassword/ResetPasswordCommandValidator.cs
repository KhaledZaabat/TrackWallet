using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    private const int MaxTokenLength = 1024;

    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.")
            .MaximumLength(MaxTokenLength).WithMessage("Reset token is invalid.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(ValidationLimits.PasswordMinLength)
                .WithMessage(ValidationMessages.PasswordTooShort)
            .Matches(ValidationPatterns.StrongPassword)
                .WithMessage(ValidationMessages.WeakPassword);

        RuleFor(x => x.UserIpAddress)
            .NotEmpty().WithMessage("IP address is required.");
    }
}
