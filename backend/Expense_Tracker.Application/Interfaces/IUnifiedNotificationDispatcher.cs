using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Interfaces;

public interface IUnifiedNotificationDispatcher
{
    Task EnqueueAsync(
        DomainNotification notification,
        CancellationToken ct);
}