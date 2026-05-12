using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class NotificationRepository(IRepository<DomainNotification> db) : INotificationRepository, IScopedService
{
    public async Task<IReadOnlyList<DomainNotification>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await db.Query()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
         DomainNotification notification,
         CancellationToken cancellationToken)
    {
        await db.AddAsync(notification, cancellationToken);
    }

    public async Task<ErrorOr<Success>> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        DomainNotification? notification = await db.Query()
            .Where(n => n.Id == notificationId && n.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
            return DomainErrors.NotificationErrors.NotFound();

        notification.MarkAsRead();

        return new Success();
    }
}
