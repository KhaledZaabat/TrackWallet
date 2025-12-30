using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;

public sealed class VerifyOtpCommandValidator
    : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator(OtpSettings otpSettings)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(ValidationPatterns.Email)
                .WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP code is required.")
            .Length(otpSettings.Digits)
                .WithMessage($"OTP code must be {otpSettings.Digits} digits.")
            .Matches(ValidationPatterns.Otp)
                .WithMessage("OTP code must contain only digits.");
    }
}