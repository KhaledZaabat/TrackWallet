using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.PushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class NotificationRepository(IRepository<DomainNotification> db)
    : INotificationRepository, IScopedService
{
    public async Task<IReadOnlyList<DomainNotification>> ListAsync(
        Guid userId,
        bool onlyUnread,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 20;
        if (take > 100) take = 100; // hard cap, prevents accidental large reads

        IQueryable<DomainNotification> q = db.Query()
            .Where(x => x.UserId == userId);

        if (onlyUnread)
            q = q.Where(x => !x.IsRead);

        return await q
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken)
    {
        return db.Query()
            .Where(x => x.UserId == userId && !x.IsRead)
            .CountAsync(cancellationToken);
    }

    public Task AddAsync(DomainNotification notification, CancellationToken cancellationToken)
    {
        return db.AddAsync(notification, cancellationToken);
    }

    public async Task<ErrorOr<Success>> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        DomainNotification? notification = await db.QueryTracked()
            .FirstOrDefaultAsync(
                n => n.Id == notificationId && n.UserId == userId,
                cancellationToken);

        if (notification is null)
            return DomainErrors.NotificationErrors.NotFound();

        notification.MarkAsRead();
        await db.SaveChangesAsync(cancellationToken);

        return new Success();
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return await db.QueryTracked()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAtUtc, now),
                cancellationToken);
    }
}
