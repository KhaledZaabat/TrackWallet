namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record BudgetHistoryItem(
    decimal Budget,
    DateTimeOffset RecordedAtUtc
);
