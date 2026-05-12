using Expense_Tracker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Infrastructure.Data;

namespace Expense_Tracker.Infrastructure.Data.Interceptors;

public sealed class SoftDeleteEntityInterceptor(IUserContext userContext)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData);
        return new(result);
    }

    private void ApplySoftDelete(DbContextEventData eventData)
    {
        if (eventData.Context is not AppDbContext context)
            return;

        if (context.DisableSoftDeleting)
            return;

        Guid deletedBy = userContext.UserId ?? Guid.Empty;

        List<EntityEntry<ISoftDeletable>> entries =
            eventData.Context.ChangeTracker
                .Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

        foreach (EntityEntry<ISoftDeletable> entry in entries)
        {
            ErrorOr<Success> result = entry.Entity.SoftDelete(deletedBy);

            if (result.IsError)
            {
                throw new InvalidOperationException(
                    result.Errors[0].Description);
            }

            entry.State = EntityState.Modified;

            foreach (ReferenceEntry owned in entry.References
                .Where(r => r.TargetEntry != null && r.TargetEntry.Metadata.IsOwned()))
            {
                owned.TargetEntry!.State = EntityState.Modified;
            }
        }
    }
}
