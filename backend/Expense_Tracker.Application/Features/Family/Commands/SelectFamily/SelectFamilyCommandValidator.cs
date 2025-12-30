using FluentValidation;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

public sealed class SelectFamilyCommandValidator : AbstractValidator<SelectFamilyCommand>
{
    public SelectFamilyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.DeviceId)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceId))
            .WithMessage("Device ID cannot exceed 500 characters.");
    }
}
