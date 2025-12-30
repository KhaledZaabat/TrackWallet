using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;

public sealed class GetFamilyBudgetHistoryQueryHandler(
    IAppDbContext db
) : IRequestHandler<GetFamilyBudgetHistoryQuery, Result<List<BudgetHistoryItem>>>
{
    public async Task<Result<List<BudgetHistoryItem>>> Handle(
        GetFamilyBudgetHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Validate months parameter
        int months = request.Months <= 0 ? 1 : request.Months;
        months = Math.Min(months, 24); // Cap at 24 months (2 years)

        // Calculate date threshold
        DateTimeOffset thresholdDate = DateTimeOffset.UtcNow.AddMonths(-months);

        // Query budget history
        var budgetHistory = await db.FamilyBudgetHistories
            .AsNoTracking()
            .Where(h =>
                h.FamilyId == request.FamilyId &&
                h.RecordedAtUtc >= thresholdDate)
            .OrderByDescending(h => h.RecordedAtUtc)
            .Select(h => new BudgetHistoryItem(
                Budget: h.Budget,
                RecordedAtUtc: h.RecordedAtUtc
            ))
            .ToListAsync(cancellationToken);



        return Result.Success(budgetHistory);
    }
}