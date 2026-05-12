
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class UserDeviceRepository(
    IRepository<UserDevice> db,
    IFcmTopicService topicService) : IUserDeviceRepository, IScopedService
{
    public async Task UpsertAsync(
        Guid userId,
        string token,
        PushPlatform platform,
        CancellationToken cancellationToken)
    {
        UserDevice? device = await db.QueryTracked()
            .SingleOrDefaultAsync(x => x.DeviceToken == token, cancellationToken);

        if (device != null)
        {
            device.BindToUser(userId);
            device.Touch();
            return;
        }

        UserDevice newDevice = UserDevice.Create(token, platform);
        newDevice.BindToUser(userId);
        await db.AddAsync(newDevice, cancellationToken);
    }

    public async Task UnbindDeviceAsync(string token, CancellationToken cancellationToken)
    {
        UserDevice? device = await db.QueryTracked()
            .SingleOrDefaultAsync(x => x.DeviceToken == token, cancellationToken);

        if (device is null)
            return;

        // Unsubscribe from all topics before unbinding
        foreach (var topic in device.SubscribedTopics.ToList())
        {
            await topicService.UnsubscribeFromTopicAsync(
                new[] { device.DeviceToken },
                topic,
                cancellationToken);
        }

        device.UnbindUser();
    }

    public async Task ClearUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<UserDevice> devices = await db.QueryTracked()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (UserDevice device in devices)
        {
            // Unsubscribe from all topics
            foreach (var topic in device.SubscribedTopics.ToList())
            {
                await topicService.UnsubscribeFromTopicAsync(
                    new[] { device.DeviceToken },
                    topic,
                    cancellationToken);
            }
            device.UnbindUser();
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await db.Query()
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.DeviceToken)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveTokenAsync(string token, CancellationToken cancellationToken)
    {
        UserDevice? device = await db.QueryTracked()
            .SingleOrDefaultAsync(x => x.DeviceToken == token, cancellationToken);

        if (device != null)
        {
            db.Remove(device);
        }
    }

    public async Task<IReadOnlyList<string>> GetUserDeviceTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await db.Query()
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.DeviceToken)
            .ToListAsync(cancellationToken);
    }

    public async Task SubscribeToTopicAsync(
        Guid userId,
        string topic,
        CancellationToken cancellationToken)
    {
        List<UserDevice> devices = await db.QueryTracked()
            .Where(x => x.UserId == userId && x.IsActive)
            .ToListAsync(cancellationToken);

        if (devices.Count == 0)
            return;

        var tokens = devices.Select(d => d.DeviceToken).ToList();

        // Subscribe to FCM topic
        await topicService.SubscribeToTopicAsync(tokens, topic, cancellationToken);

        foreach (var device in devices)
        {
            device.SubscribeToTopic(topic);
        }
    }

    public async Task UnsubscribeFromTopicAsync(
        Guid userId,
        string topic,
        CancellationToken cancellationToken)
    {
        List<UserDevice> devices = await db.QueryTracked()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (devices.Count == 0)
            return;

        var tokens = devices.Select(d => d.DeviceToken).ToList();

        // Unsubscribe from FCM topic
        await topicService.UnsubscribeFromTopicAsync(tokens, topic, cancellationToken);

        foreach (var device in devices)
        {
            device.UnsubscribeFromTopic(topic);
        }
    }
}