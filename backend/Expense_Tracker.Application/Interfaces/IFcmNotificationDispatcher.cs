using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Interfaces;

public interface IFcmNotificationDispatcher
{
    Task SendAsync(
        DomainNotification notification,
        CancellationToken cancellationToken);
}
