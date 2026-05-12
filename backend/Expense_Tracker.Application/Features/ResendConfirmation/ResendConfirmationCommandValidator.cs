using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResendConfirmation;

public class ResendConfirmationCommandValidator
    : AbstractValidator<ResendConfirmationCommand>
{
    public ResendConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
              .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
              .MaximumLength(ValidationLimits.EmailMaxLength)
              .When(x => ValidationPatterns.IsEmail(x.Email));

    }
}