using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCancelledEventHandler(
    IAppDbContext context,
    IUnifiedNotificationDispatcher dispatcher)
    : INotificationHandler<InvitationCancelledEvent>
{
    public async Task Handle(
        InvitationCancelledEvent notification,
        CancellationToken ct)
    {
        // 1. Delete related notifications for this invitation
        await context.Notifications
            .Where(n => n.UserId == notification.invitation.InviteeUserId && n.ActorUserId == notification.invitation.InviterUserId)
            .ExecuteDeleteAsync(ct);

        // 2. Delete the invitation itself
        await context.Invitations
            .Where(i => i.Id == notification.invitation.Id)
            .ExecuteDeleteAsync(ct);

        // 3. Get inviter information for the cancellation notification
        var inviterInfo = await context.Users
            .Where(u => u.Id == notification.invitation.InviterUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var familyInfo = await context.Families
            .Where(f => f.Id == notification.invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        // 4. Notify the invitee that invitation was cancelled
        DomainNotification domainNotification = DomainNotification.Create(
            userId: notification.invitation.InviteeUserId,
            title: "🚫 Invitation cancelled",
            body: $"{inviterInfo} cancelled the invitation to {familyInfo}",
            type: NotificationType.InvitationCancelled,
            actorUserId: notification.invitation.InviterUserId,
            data: new Dictionary<string, string>
            {
                [NotificationDataKeys.FAMILY_ID] = notification.invitation.FamilyId.ToString(),
                [NotificationDataKeys.INVITER_USER_ID] = notification.invitation.InviterUserId.ToString(),
                ["action"] = "none"
            });

        await dispatcher.EnqueueAsync(domainNotification, ct);
    }
}