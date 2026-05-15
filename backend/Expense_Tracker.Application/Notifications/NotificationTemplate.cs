using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Application.Notifications;

/// <summary>
/// Pre-rendered presentation for a single notification. The
/// <see cref="INotificationTemplateRegistry"/> produces one of these from a
/// <see cref="NotificationPayload"/>, capturing every field the SPA needs to
/// render and route the notification card.
/// </summary>
public sealed record NotificationTemplate(
    string Title,
    string Body,
    string IconKey,
    NotificationCategory Category,
    NotificationSeverity Severity,
    string? ResourceUri);
