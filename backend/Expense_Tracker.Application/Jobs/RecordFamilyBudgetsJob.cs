using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.FamilyUserFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ErrorOr;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Jobs;

public sealed class RecordFamilyBudgetsJob(
    IRepository<Expense_Tracker.Domain.FamilyFolder.Family> familiesRepo,
    IRepository<FamilyBudgetHistory> familyBudgetHistoriesRepo,
    ILogger<RecordFamilyBudgetsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting daily family budget recording job at {Time}", DateTimeOffset.UtcNow);

        try
        {
            List<Expense_Tracker.Domain.FamilyFolder.Family> families = await familiesRepo.QueryTracked()
                .Where(f => !f.IsDeleted)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {Count} families to record", families.Count);

            var historiesToAdd = families
                .Select(f => FamilyBudgetHistory.Create(f.Id, f.CurrentBudget, DateTimeOffset.UtcNow))
                .Where(r => !r.IsError)
                .Select(r => r.Value)
                .ToList();

            await familyBudgetHistoriesRepo.AddRangeAsync(historiesToAdd, cancellationToken);
            await familyBudgetHistoriesRepo.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Completed daily family budget recording job. Recorded {Count} histories.",
                historiesToAdd.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error in daily family budget recording job");
            throw;
        }
    }
}
