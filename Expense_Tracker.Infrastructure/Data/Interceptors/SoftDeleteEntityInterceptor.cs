using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

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
        if (eventData.Context is not IAppDbContext context)
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
            Result result = entry.Entity.SoftDelete(deletedBy);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(
                    result.TryGetError().Description);
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
