using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

public sealed class ConfirmAccountCommandValidator : AbstractValidator<ConfirmAccountCommand>
{
    // Identity tokens are HMAC-protected blobs, typically 150–250 chars.
    // 1024 leaves comfortable headroom for any token-provider change.
    private const int MaxTokenLength = 1024;

    public ConfirmAccountCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .MaximumLength(MaxTokenLength)
                .WithMessage("Verification token is invalid.");
    }
}
