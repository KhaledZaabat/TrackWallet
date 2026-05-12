using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Application.Interfaces;
using ErrorOr;
using Expense_Tracker.Contracts.Reponses.Family;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;

public sealed class GetFamilyBudgetHistoryQueryHandler(
    IRepository<FamilyBudgetHistory> familyBudgetHistoryRepo
)
{
    public async Task<ErrorOr<List<BudgetHistoryItem>>> Handle(
        GetFamilyBudgetHistoryQuery request,
        CancellationToken cancellationToken)
    {
        int months = request.Months <= 0 ? 1 : request.Months;
        months = Math.Min(months, 24); // Cap at 24 months (2 years)

        DateTimeOffset thresholdDate = DateTimeOffset.UtcNow.AddMonths(-months);

        var budgetHistory = await familyBudgetHistoryRepo.Query()
            .Where(h =>
                h.FamilyId == request.FamilyId &&
                h.RecordedAtUtc >= thresholdDate)
            .OrderByDescending(h => h.RecordedAtUtc)
            .Select(h => new BudgetHistoryItem(
                Budget: h.Budget,
                RecordedAtUtc: h.RecordedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return budgetHistory;
    }
}
