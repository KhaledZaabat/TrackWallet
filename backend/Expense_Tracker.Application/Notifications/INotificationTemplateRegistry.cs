using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Notifications;

/// <summary>
/// Single source of truth for the human-readable shape of every notification
/// type. Event handlers describe <em>what</em> happened by passing a typed
/// payload; the registry decides <em>how</em> it appears (title, body, icon,
/// deep-link, severity, category).
/// </summary>
/// <remarks>
/// Centralising rendering keeps copy edits, localisation hooks, and
/// icon-key changes out of the event-handler layer. To add a new notification
/// type, add a payload record to
/// <see cref="NotificationPayload"/>, add a case to the registry, and
/// you are done.
/// </remarks>
public interface INotificationTemplateRegistry : IScopedService
{
    NotificationTemplate Render(NotificationPayload payload);
}
