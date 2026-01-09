using FluentValidation;

namespace Expense_Tracker.Application.Features.Family.Commands.LeaveFamily;

public sealed class LeaveFamilyCommandValidator : AbstractValidator<LeaveFamilyCommand>
{
    public LeaveFamilyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");
    }
}
