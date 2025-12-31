using Expense_Tracker.Domain.CategoryFolder;

namespace Expense_Tracker.Contracts.Reponses.Category;

public sealed record CategoryResponse(
    Guid CategoryId,
    CategoryType Name);
