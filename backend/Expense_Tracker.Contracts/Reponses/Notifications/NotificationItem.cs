using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Contracts.Reponses.Notifications;

/// <summary>
/// Wire shape for a single notification entry. Mirrors the persisted
/// <see cref="DomainNotification"/> minus EF-internal fields. The
/// <see cref="Payload"/> is serialised polymorphically (System.Text.Json
/// $kind discriminator) so the SPA can switch on it.
/// </summary>
public sealed record NotificationItem(
    Guid Id,
    NotificationType Type,
    NotificationCategory Category,
    NotificationSeverity Severity,
    string Title,
    string Body,
    string IconKey,
    string? ResourceUri,
    bool IsRead,
    DateTimeOffset? ReadAtUtc,
    DateTimeOffset CreatedAtUtc,
    Guid? ActorUserId,
    NotificationPayload? Payload);
