using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.External_Providers.Commands.MobileGoogleOauth;

public sealed class GoogleMobileLoginCommandValidator
    : AbstractValidator<GoogleMobileLoginCommand>
{
    public GoogleMobileLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage(ValidationMessages.IdTokenRequired)
            .MaximumLength(ValidationLimits.IdTokenMaxLength);

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage(ValidationMessages.DeviceIdRequired)
            .MaximumLength(ValidationLimits.DeviceIdMaxLength);

        RuleFor(x => x.FcmToken)
            .MaximumLength(ValidationLimits.FcmTokenMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.FcmToken))
            .WithMessage(ValidationMessages.InvalidFcmToken);
    }
}