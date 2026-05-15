using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Notifications;

/// <summary>
/// Convenience surface event handlers use to create a notification from a
/// strongly-typed payload. Internally delegates rendering to
/// <see cref="INotificationTemplateRegistry"/>.
/// </summary>
public interface INotificationBuilder : IScopedService
{
    DomainNotification Build(
        Guid recipientUserId,
        Guid? actorUserId,
        NotificationPayload payload);
}
