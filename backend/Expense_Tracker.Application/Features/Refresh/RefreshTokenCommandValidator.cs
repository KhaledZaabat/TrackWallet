using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .MaximumLength(512);

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("DeviceId is required.")
            .MaximumLength(128);

        RuleFor(x => x.FcmToken)
         .MaximumLength(ValidationLimits.FcmTokenMaxLength)
         .When(x => !string.IsNullOrWhiteSpace(x.FcmToken))
         .WithMessage(ValidationMessages.InvalidFcmToken);
    }
}