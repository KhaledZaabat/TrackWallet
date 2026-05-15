using System.Text.Json;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using FirebaseAdmin.Messaging;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class FcmTopicService : IFcmTopicService, IScopedService
{
    private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SubscribeToTopicAsync(
        IEnumerable<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var tokens = deviceTokens.ToList();
        if (tokens.Count == 0)
            return;

        await FirebaseMessaging.DefaultInstance
            .SubscribeToTopicAsync(tokens, SanitizeTopicName(topic));
    }

    public async Task UnsubscribeFromTopicAsync(
        IEnumerable<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var tokens = deviceTokens.ToList();
        if (tokens.Count == 0)
            return;

        await FirebaseMessaging.DefaultInstance
            .UnsubscribeFromTopicAsync(tokens, SanitizeTopicName(topic));
    }

    public async Task SendToTopicAsync(
        string topic,
        DomainNotification notification,
        CancellationToken cancellationToken = default)
    {
        Message message = new()
        {
            Topic = SanitizeTopicName(topic),
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
            },
            Data = BuildData(notification),
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }

    private static Dictionary<string, string> BuildData(DomainNotification notification)
    {
        var data = new Dictionary<string, string>(capacity: 6)
        {
            ["notificationId"] = notification.Id.ToString(),
            ["type"] = notification.Type.ToString(),
            ["category"] = notification.Category.ToString(),
            ["severity"] = notification.Severity.ToString(),
            ["iconKey"] = notification.IconKey,
        };

        if (!string.IsNullOrEmpty(notification.ResourceUri))
            data["resourceUri"] = notification.ResourceUri;

        if (notification.Payload is not null)
            data["payload"] = JsonSerializer.Serialize(notification.Payload, PayloadJsonOptions);

        return data;
    }

    private static string SanitizeTopicName(string topic) => topic.Replace("-", "_");
}
