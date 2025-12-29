using FluentValidation;
using Expense_Tracker.Application.Constans;

namespace Expense_Tracker.Application.Features.UpdatePassword;

public sealed class UpdatePasswordCommandValidator
    : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {


        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Matches(ValidationPatterns.StrongPassword).WithMessage(ValidationMessages.WeakPassword);

        RuleFor(x => x.NewPassword)
             .NotEmpty().WithMessage(ValidationMessages.Required)
            .Matches(ValidationPatterns.StrongPassword).WithMessage(ValidationMessages.WeakPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password cannot be the same as the current password.");

        RuleFor(x => x.UserIpAddress)
            .NotEmpty().WithMessage("User IP Address is required.");
    }
}