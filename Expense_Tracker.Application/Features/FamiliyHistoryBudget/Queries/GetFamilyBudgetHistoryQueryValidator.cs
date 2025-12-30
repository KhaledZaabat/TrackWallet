using FluentValidation;

namespace Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;

public sealed class GetFamilyBudgetHistoryQueryValidator : AbstractValidator<GetFamilyBudgetHistoryQuery>
{
    public GetFamilyBudgetHistoryQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.Months)
            .InclusiveBetween(1, 24)
            .WithMessage("Months must be between 1 and 24.");
    }
}