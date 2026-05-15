using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Infrastructure.Data;
using Hangfire;

namespace Expense_Tracker.Infrastructure.NotificationsDispatcher;

/// <summary>
/// Persists the in-product notification and queues a background job that pushes
/// it to FCM (mobile + web push). Persistence is committed before the job is
/// queued so the dispatch worker can rely on the row existing when it runs.
/// </summary>
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
        await context.SaveChangesAsync(ct);

        backgroundJobs.Enqueue<NotificationDispatchJob>(
            job => job.ExecuteAsync(notification.Id, CancellationToken.None));
    }
}
