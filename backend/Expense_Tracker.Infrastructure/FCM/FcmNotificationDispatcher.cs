using System.Text.Json;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using FirebaseAdmin.Messaging;

namespace Expense_Tracker.Infrastructure.FCM;

/// <summary>
/// Forwards an in-product notification to FCM for any device the user has
/// registered. Mobile and web push are both served from a single topic-less
/// multicast: the SPA's web-push subscription, when registered as a device
/// token, lands here too.
/// </summary>
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

        // Reap stale tokens. FCM responds with Unregistered when a token has been
        // revoked client-side; keeping it would just waste another round-trip.
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

    /// <summary>
    /// Flattens the typed payload to FCM's <c>data</c> map. FCM only allows
    /// string-string entries, so we serialise the payload as a single JSON
    /// blob the SPA service-worker can <c>JSON.parse</c>.
    /// </summary>
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
