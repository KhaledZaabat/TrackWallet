using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Domain.PushNotifications;

public sealed class UserDevice : Entity, IAuditable
{
    public Guid? UserId { get; private set; }
    public string DeviceToken { get; private set; }
    public PushPlatform Platform { get; private set; }
    public bool IsActive { get; private set; }
    public List<string> SubscribedTopics { get; private set; } = new();
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }
    public Guid LastModifiedBy { get; set; }

    private UserDevice() { }

    private UserDevice(Guid id, string deviceToken, PushPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            throw new ArgumentException("DeviceToken is required.", nameof(deviceToken));

        Id = id;
        DeviceToken = deviceToken;
        Platform = platform;
        IsActive = true;
        SubscribedTopics = new List<string>();
    }

    public static UserDevice Create(string deviceToken, PushPlatform platform)
    {
        return new UserDevice(Guid.CreateVersion7(), deviceToken, platform);
    }

    public void BindToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.");

        UserId = userId;
        IsActive = true;
    }

    public void UnbindUser()
    {
        UserId = null;
        SubscribedTopics.Clear();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Touch()
    {
        LastModifiedUtc = DateTimeOffset.UtcNow;
    }

    public ErrorOr<Success> SubscribeToTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UserDevice), "Topic cannot be empty.");

        if (!SubscribedTopics.Contains(topic))
            SubscribedTopics.Add(topic);

        Touch();
        return new Success();
    }

    public void UnsubscribeFromTopic(string topic)
    {
        SubscribedTopics.Remove(topic);
        Touch();
    }

    public void ClearTopics()
    {
        SubscribedTopics.Clear();
        Touch();
    }
}
