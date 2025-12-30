using Expense_Tracker.Domain.Common;
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
}