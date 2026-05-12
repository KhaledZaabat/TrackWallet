using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RawRefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .MaximumLength(512);

        RuleFor(x => x.FcmToken)
         .MaximumLength(ValidationLimits.FcmTokenMaxLength)
         .When(x => !string.IsNullOrWhiteSpace(x.FcmToken))
         .WithMessage(ValidationMessages.InvalidFcmToken);
    }
}
