using FluentValidation;

namespace Expense_Tracker.Application.Features.Family.Commands.UpdateFamily;

public sealed class UpdateFamilyCommandValidator : AbstractValidator<UpdateFamilyCommand>
{
    public UpdateFamilyCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Family name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.FamilyBio)
            .MaximumLength(500)
            .WithMessage("Family bio cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.FamilyBio));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Name) || x.FamilyBio != null)
            .WithMessage("At least one field (Name or FamilyBio) must be provided for update.");
    }
}
