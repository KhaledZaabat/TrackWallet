using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Notifications;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Features.Notifications.ListNotifications;

public sealed class ListNotificationsQueryHandler(
    INotificationRepository notifications,
    IUserContext userContext)
{
    public async Task<ErrorOr<NotificationListResponse>> Handle(
        ListNotificationsQuery query,
        CancellationToken ct)
    {
        if (userContext.UserId is not { } userId)
            return DomainErrors.UserErrors.NotFound();

        IReadOnlyList<DomainNotification> rows = await notifications.ListAsync(
            userId,
            query.OnlyUnread,
            query.Skip,
            query.Take,
            ct);

        // The unread count is shown as a badge alongside the list, so issuing it
        // here in the same trip avoids a follow-up round-trip from the SPA.
        int unreadCount = await notifications.CountUnreadAsync(userId, ct);

        var items = rows
            .Select(n => new NotificationItem(
                Id: n.Id,
                Type: n.Type,
                Category: n.Category,
                Severity: n.Severity,
                Title: n.Title,
                Body: n.Body,
                IconKey: n.IconKey,
                ResourceUri: n.ResourceUri,
                IsRead: n.IsRead,
                ReadAtUtc: n.ReadAtUtc,
                CreatedAtUtc: n.CreatedAtUtc,
                ActorUserId: n.ActorUserId,
                Payload: n.Payload))
            .ToList();

        return new NotificationListResponse(items, unreadCount, query.Skip, query.Take);
    }
}
