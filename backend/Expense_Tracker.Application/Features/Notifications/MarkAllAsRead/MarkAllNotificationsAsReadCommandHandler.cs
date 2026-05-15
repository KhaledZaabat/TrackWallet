using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Notifications;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Notifications.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler(
    INotificationRepository notifications,
    IUserContext userContext)
{
    public async Task<ErrorOr<MarkAllAsReadResponse>> Handle(
        MarkAllNotificationsAsReadCommand _,
        CancellationToken ct)
    {
        if (userContext.UserId is not { } userId)
            return DomainErrors.UserErrors.NotFound();

        int updated = await notifications.MarkAllAsReadAsync(userId, ct);
        return new MarkAllAsReadResponse(updated);
    }
}
