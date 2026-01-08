using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCancelledEventHandler(
    IAppDbContext context,
    IUnifiedNotificationDispatcher dispatcher,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
    : INotificationHandler<InvitationCancelledEvent>
{
    public async Task Handle(
        InvitationCancelledEvent notification,
        CancellationToken ct)
    {
        // Get inviter, invitee, and family information
        var inviterInfo = await context.Users
            .Where(u => u.Id == notification.invitation.InviterUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviteeInfo = await context.Users
            .Where(u => u.Id == notification.invitation.InviteeUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
            .SingleOrDefaultAsync(ct);

        var familyInfo = await context.Families
            .Where(f => f.Id == notification.invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        // Send email BEFORE deleting (if enabled)
        if (inviteeInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviteeInfo.Email))
        {
            await SendCancelledEmailAsync(
                inviteeInfo.Email,
                inviteeInfo.UserName,
                inviterInfo,
                familyInfo,
                ct);
        }

        // 1. Delete related notifications for this invitation
        await context.Notifications
            .Where(n => n.UserId == notification.invitation.InviteeUserId
                     && n.ActorUserId == notification.invitation.InviterUserId)
            .ExecuteDeleteAsync(ct);

        // 2. Delete the invitation itself
        await context.Invitations
            .Where(i => i.Id == notification.invitation.Id)
            .ExecuteDeleteAsync(ct);

        // 3. Notify the invitee that invitation was cancelled
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

    private async Task SendCancelledEmailAsync(
        string email,
        string inviteeName,
        string inviterName,
        string familyName,
        CancellationToken ct)
    {
        var template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationCancelledTemplate,
            ct);

        var body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["InviteeName"] = inviteeName,
            ["InviterName"] = inviterName,
            ["FamilyName"] = familyName,
            ["AppLink"] = "expensetracker://invitations"
        });

        await notification.SendEmailAsync(
            to: email,
            subject: $"🚫 Invitation cancelled - {familyName}",
            htmBody: body,
            cancellationToken: ct);
    }
}