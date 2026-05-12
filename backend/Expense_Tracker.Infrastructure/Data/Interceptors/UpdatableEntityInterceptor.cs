using Expense_Tracker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Infrastructure.Data.Interceptors;

public sealed class UpdatableEntityInterceptor(IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyUpdateAudit(eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyUpdateAudit(eventData);
        return new(result);
    }

    private void ApplyUpdateAudit(DbContextEventData eventData)
    {
        if (eventData.Context is not AppDbContext context)
            return;

        if (context.DisableUpdateAudit)
            return;

        Guid userId = userContext.UserId ?? Guid.Empty;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var entries = eventData.Context.ChangeTracker
            .Entries<IUpdatable>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.LastModifiedUtc = now;
            entry.Entity.LastModifiedBy = userId;
        }
    }
}
