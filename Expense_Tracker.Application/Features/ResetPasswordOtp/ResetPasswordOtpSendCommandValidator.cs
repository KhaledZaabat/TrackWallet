using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Features.Identity.Commands.ForgotPassword;
using FluentValidation;

public class ResetPasswordOtpSendCommandValidator : AbstractValidator<ResetPasswordOtpSendCommand>
{
    public ResetPasswordOtpSendCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email required");


        RuleFor(x => x.Email)
            .Matches(ValidationPatterns.Email).WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength);



    }
}
