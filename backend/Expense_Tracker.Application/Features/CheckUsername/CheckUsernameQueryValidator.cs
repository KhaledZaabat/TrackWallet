using Expense_Tracker.Application.Constans;
using FluentValidation;

namespace Expense_Tracker.Application.Features.CheckUsername;

public sealed class CheckUsernameQueryValidator : AbstractValidator<CheckUsernameQuery>
{
    public CheckUsernameQueryValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ValidationMessages.UserNameRequired)
            .MinimumLength(ValidationLimits.UserNameMinLength)
                .WithMessage(ValidationMessages.InvalidUserName)
            .MaximumLength(ValidationLimits.UserNameMaxLength)
                .WithMessage(ValidationMessages.InvalidUserName)
            .Matches(ValidationPatterns.UserName)
                .WithMessage(ValidationMessages.InvalidUserName);
    }
}
