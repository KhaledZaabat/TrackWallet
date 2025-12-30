using FluentValidation;

namespace Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;

public sealed class GetFamilyTransactionsQueryValidator : AbstractValidator<GetFamilyTransactionsQuery>
{
    public GetFamilyTransactionsQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.Cursor)
            .Must(BeValidDateTimeOffset)
            .When(x => !string.IsNullOrWhiteSpace(x.Cursor))
            .WithMessage("Cursor must be a valid ISO 8601 date-time string.");
    }

    private bool BeValidDateTimeOffset(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return true;

        return DateTimeOffset.TryParse(cursor, out _);
    }
}
