using FluentValidation;

namespace Expense_Tracker.Application.Features.Family.Commands.CreateFamily;

public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Family name is required.")
            .MaximumLength(100)
            .WithMessage("Family name cannot exceed 100 characters.");

        RuleFor(x => x.InitialBudget)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial budget cannot be negative.")
            .LessThanOrEqualTo(999999999.99m)
            .WithMessage("Initial budget is too large.");

        RuleFor(x => x.FamilyBio)
            .MaximumLength(500)
            .WithMessage("Family bio cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.FamilyBio));
    }
}