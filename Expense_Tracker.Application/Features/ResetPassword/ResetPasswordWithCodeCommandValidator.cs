using FluentValidation;
using Expense_Tracker.Application.Constans;

namespace Expense_Tracker.Application.Features.ResetPassword
{
    public sealed class ResetPasswordWithCodeCommandValidator
        : AbstractValidator<ResetPasswordWithCodeCommand>
    {
        public ResetPasswordWithCodeCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessages.Required);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Reset code is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(ValidationMessages.Required)
                 .Matches(ValidationPatterns.StrongPassword).WithMessage(ValidationMessages.WeakPassword);
        }
    }
}