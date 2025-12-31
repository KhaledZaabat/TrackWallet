namespace Expense_Tracker.Contracts.Reponses.Category;

public sealed record CategoryResponse(
    Guid CategoryId,
    string Name);
