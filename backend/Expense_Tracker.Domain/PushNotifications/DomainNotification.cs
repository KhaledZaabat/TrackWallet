using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Domain.PushNotifications;


public sealed class DomainNotification : Entity, ICreatable
{
    public Guid UserId { get; private set; }          // recipient
    public Guid? ActorUserId { get; private set; }    // who caused it (nullable for system events)

    public NotificationType Type { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationSeverity Severity { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    /// <summary>Stable icon key the SPA maps to a concrete asset (e.g. "invitation").</summary>
    public string IconKey { get; private set; } = string.Empty;

    /// <summary>App-relative URI for deep-linking. Never an external URL.</summary>
    public string? ResourceUri { get; private set; }

    /// <summary>Strongly-typed payload, persisted as JSONB.</summary>
    public NotificationPayload? Payload { get; private set; }

    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }

    private DomainNotification() { } // EF

    private DomainNotification(
        Guid id,
        Guid userId,
        Guid? actorUserId,
        NotificationType type,
        NotificationCategory category,
        NotificationSeverity severity,
        string title,
        string body,
        string iconKey,
        string? resourceUri,
        NotificationPayload? payload)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (actorUserId == Guid.Empty)
            throw new ArgumentException("ActorUserId cannot be Guid.Empty; pass null instead.", nameof(actorUserId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));

        if (string.IsNullOrWhiteSpace(iconKey))
            throw new ArgumentException("IconKey is required.", nameof(iconKey));

        Id = id;
        UserId = userId;
        ActorUserId = actorUserId;
        Type = type;
        Category = category;
        Severity = severity;
        Title = title.Trim();
        Body = body.Trim();
        IconKey = iconKey.Trim();
        ResourceUri = string.IsNullOrWhiteSpace(resourceUri) ? null : resourceUri.Trim();
        Payload = payload;
        IsRead = false;
    }

    public static DomainNotification Create(
        Guid userId,
        Guid? actorUserId,
        NotificationType type,
        NotificationCategory category,
        NotificationSeverity severity,
        string title,
        string body,
        string iconKey,
        string? resourceUri,
        NotificationPayload? payload)
    {
        return new DomainNotification(
            Guid.CreateVersion7(),
            userId,
            actorUserId,
            type,
            category,
            severity,
            title,
            body,
            iconKey,
            resourceUri,
            payload);
    }

    public void MarkAsRead()
    {
        if (IsRead) return;

        IsRead = true;
        ReadAtUtc = DateTimeOffset.UtcNow;
    }
}
