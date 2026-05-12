using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Infrastructure.Data;
using Hangfire;

namespace Expense_Tracker.Infrastructure.NotificationsDispatcher;

public sealed class UnifiedNotificationDispatcher(
    AppDbContext context,
    IBackgroundJobClient backgroundJobs)
    : IUnifiedNotificationDispatcher, IScopedService
{
    public async Task EnqueueAsync(
        DomainNotification notification,
        CancellationToken ct)
    {
        context.Notifications.Add(notification);
        backgroundJobs.Enqueue<NotificationDispatchJob>(
            job => job.ExecuteAsync(notification.Id, CancellationToken.None));
    }
}
