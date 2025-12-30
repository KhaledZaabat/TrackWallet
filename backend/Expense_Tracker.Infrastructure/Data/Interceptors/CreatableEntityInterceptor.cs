using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Infrastructure.Data.Interceptors;

public sealed class CreatableEntityInterceptor(IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyCreationAudit(eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyCreationAudit(eventData);
        return new(result);
    }

    private void ApplyCreationAudit(DbContextEventData eventData)
    {
        if (eventData.Context is not IAppDbContext context)
            return;

        if (context.DisableCreationAudit)
            return;

        Guid userId = userContext.UserId ?? Guid.Empty;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var entries = eventData.Context.ChangeTracker
            .Entries<ICreatable>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            entry.Entity.CreatedAtUtc = now;
            entry.Entity.CreatedBy = userId;
        }
    }
}
