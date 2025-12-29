

using Expense_Tracker.Application.Interfaces;
using FirebaseAdmin.Messaging;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class FcmNotificationDispatcher(IUserDeviceRepository devices) : IFcmNotificationDispatcher, IScopedService
{


    public async Task SendAsync(
        DomainNotification notification,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> tokens =
            await devices.GetActiveTokensAsync(
                notification.UserId,
                cancellationToken);

        if (tokens.Count == 0)
            return;

        MulticastMessage message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body
            },
            Data = BuildData(notification)
        };

        BatchResponse response =
            await FirebaseMessaging
                .DefaultInstance
                .SendEachForMulticastAsync(message, cancellationToken);

        for (int i = 0; i < response.Responses.Count; i++)
        {
            if (!response.Responses[i].IsSuccess &&
                response.Responses[i].Exception?.MessagingErrorCode ==
                MessagingErrorCode.Unregistered)
            {
                await devices.RemoveTokenAsync(
                    tokens[i],
                    cancellationToken);
            }
        }
    }

    private static Dictionary<string, string> BuildData(DomainNotification notification)
    {
        Dictionary<string, string> data =
            notification.Data ?? new Dictionary<string, string>();

        data["notificationId"] = notification.Id.ToString();
        data["type"] = notification.Type.ToString();

        return data;
    }
}