using Expense_Tracker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class NotificationDispatchJob(
IAppDbContext context,
IFcmNotificationDispatcher fcmDispatcher) : ITransientService
{
    public async Task ExecuteAsync(Guid notificationId, CancellationToken ct)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification is null) return;

        await fcmDispatcher.SendAsync(notification, ct);
    }
}