using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCreatedEventHandler(
    IAppDbContext context,
    IUnifiedNotificationDispatcher dispatcher)
    : INotificationHandler<InvitationCreatedEvent>
{
    public async Task Handle(
        InvitationCreatedEvent notification,
        CancellationToken ct)
    {
        // Get inviter and family information
        var inviterInfo = await context.Users
            .Where(u => u.Id == notification.Invitation.InviterUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var familyInfo = await context.Families
            .Where(f => f.Id == notification.Invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        DomainNotification domainNotification = DomainNotification.Create(
            userId: notification.Invitation.InviteeUserId,
            title: "👨‍👩‍👧‍👦 New family invitation",
            body: $"{inviterInfo} invited you to join {familyInfo}",
            type: NotificationType.FamilyInvitation,
            actorUserId: notification.Invitation.InviterUserId,
            data: new Dictionary<string, string>
            {
                [NotificationDataKeys.INVITATION_ID] = notification.Invitation.Id.ToString(),
                [NotificationDataKeys.FAMILY_ID] = notification.Invitation.FamilyId.ToString(),
                [NotificationDataKeys.INVITER_USER_ID] = notification.Invitation.InviterUserId.ToString(),
                ["action"] = "open-invitations"
            });

        await dispatcher.EnqueueAsync(domainNotification, ct);
    }
}