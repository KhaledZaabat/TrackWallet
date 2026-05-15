using System.Text.Json;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using FirebaseAdmin.Messaging;

namespace Expense_Tracker.Infrastructure.FCM;


public sealed class FcmNotificationDispatcher(IUserDeviceRepository devices)
    : IFcmNotificationDispatcher, IScopedService
{
    private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SendAsync(
        DomainNotification notification,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> tokens =
            await devices.GetActiveTokensAsync(notification.UserId, cancellationToken);

        if (tokens.Count == 0)
            return;

        MulticastMessage message = new()
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
            },
            Data = BuildData(notification),
        };

        BatchResponse response = await FirebaseMessaging
            .DefaultInstance
            .SendEachForMulticastAsync(message, cancellationToken);

        for (int i = 0; i < response.Responses.Count; i++)
        {
            SendResponse r = response.Responses[i];
            if (!r.IsSuccess &&
                r.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                await devices.RemoveTokenAsync(tokens[i], cancellationToken);
            }
        }
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
}
