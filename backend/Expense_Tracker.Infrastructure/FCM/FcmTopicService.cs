
using Expense_Tracker.Application.Interfaces;
using FirebaseAdmin.Messaging;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class FcmTopicService()
    : IFcmTopicService, IScopedService
{
    public async Task SubscribeToTopicAsync(
        IEnumerable<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var tokens = deviceTokens.ToList();
        if (tokens.Count == 0)
            return;

        var sanitizedTopic = SanitizeTopicName(topic);

        TopicManagementResponse response = await FirebaseMessaging
            .DefaultInstance
            .SubscribeToTopicAsync(tokens, sanitizedTopic);



    }

    public async Task UnsubscribeFromTopicAsync(
        IEnumerable<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var tokens = deviceTokens.ToList();
        if (tokens.Count == 0)
            return;

        var sanitizedTopic = SanitizeTopicName(topic);

        await FirebaseMessaging
            .DefaultInstance
            .UnsubscribeFromTopicAsync(tokens, sanitizedTopic);
    }

    public async Task SendToTopicAsync(
        string topic,
        DomainNotification notification,
        CancellationToken cancellationToken = default)
    {
        var sanitizedTopic = SanitizeTopicName(topic);

        Message message = new()
        {
            Topic = sanitizedTopic,
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body
            },
            Data = BuildData(notification)
        };

        await FirebaseMessaging
            .DefaultInstance
            .SendAsync(message, cancellationToken);
    }

    private static Dictionary<string, string> BuildData(DomainNotification notification)
    {
        Dictionary<string, string> data =
            notification.Data ?? new Dictionary<string, string>();

        data["notificationId"] = notification.Id.ToString();
        data["type"] = notification.Type.ToString();

        return data;
    }

    private static string SanitizeTopicName(string topic)
    {
        return topic.Replace("-", "_");
    }
}

