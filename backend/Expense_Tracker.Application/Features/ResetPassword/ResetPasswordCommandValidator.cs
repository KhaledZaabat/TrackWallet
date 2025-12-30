using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator
    : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        // Email rules
        RuleFor(x => x.Email)
            .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);


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