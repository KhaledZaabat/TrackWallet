using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Expense_Tracker.Application.Jobs;

public sealed class RecordFamilyBudgetsJob(
    IAppDbContext db,
    ILogger<RecordFamilyBudgetsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting daily family budget recording job at {Time}", DateTimeOffset.UtcNow);

        try
        {
            // Get all active families
            List<Family> families = await db.Families
                .Where(f => !f.IsDeleted)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {Count} families to record", families.Count);


            var historiesToAdd = families
                .Select(f => FamilyBudgetHistory.Create(f.Id, f.CurrentBudget, DateTimeOffset.UtcNow))
                .Where(r => r.IsSuccess)
                .Select(r => r.TryGetValue())
                .ToList();

            // Add all at once
            db.FamilyBudgetHistories.AddRange(historiesToAdd);
            await db.SaveChangesAsync(cancellationToken);

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
