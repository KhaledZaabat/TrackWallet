using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Notifications;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Notifications.UnreadCount;

public sealed class UnreadCountQueryHandler(
    INotificationRepository notifications,
    IUserContext userContext)
{
    public async Task<ErrorOr<UnreadCountResponse>> Handle(
        UnreadCountQuery _,
        CancellationToken ct)
    {
        if (userContext.UserId is not { } userId)
            return DomainErrors.UserErrors.NotFound();

        int unread = await notifications.CountUnreadAsync(userId, ct);
        return new UnreadCountResponse(unread);
    }
}
