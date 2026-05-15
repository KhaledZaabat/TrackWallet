using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Notifications.MarkAsRead;

public sealed class MarkNotificationAsReadCommandHandler(
    INotificationRepository notifications,
    IUserContext userContext)
{
    public Task<ErrorOr<Success>> Handle(
        MarkNotificationAsReadCommand command,
        CancellationToken ct)
    {
        if (userContext.UserId is not { } userId)
            return Task.FromResult<ErrorOr<Success>>(DomainErrors.UserErrors.NotFound());

        return notifications.MarkAsReadAsync(command.NotificationId, userId, ct);
    }
}
