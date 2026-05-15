using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.CheckUsername;

public sealed class CheckUsernameQueryValidator : AbstractValidator<CheckUsernameQuery>
{
    public CheckUsernameQueryValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MaximumLength(ValidationLimits.UserNameMaxLength)
                .WithMessage($"Username cannot exceed {ValidationLimits.UserNameMaxLength} characters.")
            .Matches(@"^[a-zA-Z0-9_-]+$")
                .WithMessage("Username can only contain letters, numbers, underscores, and hyphens.");
    }
}
