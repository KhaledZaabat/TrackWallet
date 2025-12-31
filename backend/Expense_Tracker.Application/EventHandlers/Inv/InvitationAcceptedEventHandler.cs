using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationAcceptedEventHandler(
    IAppDbContext context,
    IUnifiedNotificationDispatcher dispatcher)
    : INotificationHandler<InvitationAcceptedEvent>
{
    public async Task Handle(
        InvitationAcceptedEvent notification,
        CancellationToken ct)
    {
        // Get invitee and family information
        var inviteeInfo = await context.Users
            .Where(u => u.Id == notification.Invitation.InviteeUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var familyInfo = await context.Families
            .Where(f => f.Id == notification.Invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        // Notify the inviter
        DomainNotification domainNotification = DomainNotification.Create(
            userId: notification.Invitation.InviterUserId,
            title: "✅ Invitation accepted",
            body: $"{inviteeInfo} accepted your invitation to {familyInfo}",
            type: NotificationType.InvitationAccepted,
            actorUserId: notification.Invitation.InviteeUserId,
            data: new Dictionary<string, string>
            {
                [NotificationDataKeys.INVITATION_ID] = notification.Invitation.Id.ToString(),
                [NotificationDataKeys.FAMILY_ID] = notification.Invitation.FamilyId.ToString(),
                [NotificationDataKeys.INVITEE_USER_ID] = notification.Invitation.InviteeUserId.ToString(),
                ["action"] = "open-family"
            });

        await dispatcher.EnqueueAsync(domainNotification, ct);
    }
}