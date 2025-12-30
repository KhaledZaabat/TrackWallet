using Expense_Tracker.Application.Constans;
using FluentValidation;
namespace Expense_Tracker.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandValidator
    : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage(ValidationMessages.DeviceIdRequired)
            .MaximumLength(ValidationLimits.DeviceIdMaxLength);

        RuleFor(x => x.FcmToken)
            .MaximumLength(ValidationLimits.FcmTokenMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.FcmToken))
            .WithMessage(ValidationMessages.InvalidFcmToken);
    }
}