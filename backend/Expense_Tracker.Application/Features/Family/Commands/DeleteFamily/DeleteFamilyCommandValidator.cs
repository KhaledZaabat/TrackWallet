using FluentValidation;

namespace Expense_Tracker.Application.Features.Family.Commands.DeleteFamily;

public sealed class DeleteFamilyCommandValidator : AbstractValidator<DeleteFamilyCommand>
{
    public DeleteFamilyCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user ID is required.");
    }
}
