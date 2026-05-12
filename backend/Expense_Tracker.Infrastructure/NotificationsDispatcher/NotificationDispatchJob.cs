using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.NotificationsDispatcher;

public sealed class NotificationDispatchJob(
    AppDbContext context,
    IFcmNotificationDispatcher fcmDispatcher) : ITransientService
{
    public async Task ExecuteAsync(Guid notificationId, CancellationToken ct)
    {
        var notification = await context.Notifications
            .FindAsync(notificationId, ct);

        if (notification is null) return;

        await fcmDispatcher.SendAsync(notification, ct);
    }
}
