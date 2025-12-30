using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.PushNotifications.Enums;

public sealed class DomainNotification : Entity
{
    public Guid UserId { get; private set; }          // Receiver
    public Guid? ActorUserId { get; private set; }    // Actor (who caused it)

    public string Title { get; private set; }
    public string Body { get; private set; }
    public Dictionary<string, string>? Data { get; private set; }

    public bool IsRead { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public NotificationType Type { get; private set; }

    private DomainNotification() { } // EF

    private DomainNotification(
        Guid id,
        Guid userId,
        Guid? actorUserId,
        string title,
        string body,
        NotificationType type,
        Dictionary<string, string>? data)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        if (actorUserId == Guid.Empty)
            throw new ArgumentException("ActorUserId cannot be empty Guid.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.");

        Id = id;
        UserId = userId;
        ActorUserId = actorUserId;
        Title = title.Trim();
        Body = body.Trim();
        Type = type;
        Data = data;

        IsRead = false;
        CreatedUtc = DateTime.UtcNow;
    }

    public static DomainNotification Create(
        Guid userId,
        string title,
        string body,
        NotificationType type,
        Guid? actorUserId = null,
        Dictionary<string, string>? data = null)
    {
        return new DomainNotification(
            Guid.CreateVersion7(),
            userId,
            actorUserId,
            title,
            body,
            type,
            data);
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
    }
}
