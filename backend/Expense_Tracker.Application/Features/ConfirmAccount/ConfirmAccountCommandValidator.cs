using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

public sealed class ConfirmAccountCommandValidator
    : AbstractValidator<ConfirmAccountCommand>
{
    public ConfirmAccountCommandValidator(OtpSettings otpSettings)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        // Email rules
        RuleFor(x => x.Email)
            .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);

        // Phone rules

        RuleFor(x => x.Otp)
                   .NotEmpty().WithMessage("OTP code is required.")
                   .Length(otpSettings.Digits).WithMessage($"OTP code must be {otpSettings.Digits} ")
                   .Matches(ValidationPatterns.Otp).WithMessage("OTP code must contain only digits.");
    }
}
