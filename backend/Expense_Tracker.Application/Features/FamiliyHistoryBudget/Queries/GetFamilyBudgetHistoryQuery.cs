using Expense_Tracker.Contracts.Reponses.Family;

namespace Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;

public sealed record GetFamilyBudgetHistoryQuery(
    Guid FamilyId,
    int Months = 1
);
