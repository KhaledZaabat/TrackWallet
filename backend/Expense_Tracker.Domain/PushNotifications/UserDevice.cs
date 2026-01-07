using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Domain.PushNotifications;

public sealed class UserDevice : Entity
{
    public Guid? UserId { get; private set; }
    public string DeviceToken { get; private set; }
    public PushPlatform Platform { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime? LastSeenUtc { get; private set; }
    public List<string> SubscribedTopics { get; private set; } = new();

    private UserDevice() { } // EF

    private UserDevice(Guid id, string deviceToken, PushPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            throw new ArgumentException("DeviceToken is required.", nameof(deviceToken));

        Id = id;
        DeviceToken = deviceToken;
        Platform = platform;
        IsActive = true;
        CreatedUtc = DateTime.UtcNow;
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
        LastSeenUtc = DateTime.UtcNow;
    }

    public void UnbindUser()
    {
        UserId = null;
        SubscribedTopics.Clear(); // Clear topics when unbinding
        LastSeenUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastSeenUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastSeenUtc = DateTime.UtcNow;
    }

    public void Touch()
    {
        LastSeenUtc = DateTime.UtcNow;
    }

    public Result SubscribeToTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            Result.Failure(DomainError.InvalidState(nameof(UserDevice), "topic can not be empty"));
        if (!SubscribedTopics.Contains(topic))
        {
            SubscribedTopics.Add(topic);
        }
        Touch();
        return Result.Success();
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