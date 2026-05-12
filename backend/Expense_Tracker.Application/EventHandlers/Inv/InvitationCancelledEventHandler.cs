using User = Expense_Tracker.Domain.Users.User;
using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.PushNotifications.Enums;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCancelledEventHandler(
    IRepository<global::Expense_Tracker.Domain.Users.User> users,
    IRepository<Family> families,
    IRepository<DomainNotification> notifications,
    IRepository<Invitation> invitations,
    IUnifiedNotificationDispatcher dispatcher,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    public async Task Handle(
        InvitationCancelledEvent notification,
        CancellationToken ct)
    {
        // Get inviter, invitee, and family information
        var inviterInfo = await users.Query()
            .Where(u => u.Id == notification.Invitation.InviterUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviteeInfo = await users.Query()
            .Where(u => u.Id == notification.Invitation.InviteeUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
            .SingleOrDefaultAsync(ct);

        var familyInfo = await families.Query()
            .Where(f => f.Id == notification.Invitation.FamilyId)
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
        var relatedNotifications = await notifications.QueryTracked()
            .Where(n => n.UserId == notification.Invitation.InviteeUserId
                     && n.ActorUserId == notification.Invitation.InviterUserId)
            .ToListAsync(ct);

        if (relatedNotifications.Count > 0)
            notifications.RemoveRange(relatedNotifications);

        // 2. Delete the invitation itself
        var invitationToDelete = await invitations.QueryTracked()
            .FirstOrDefaultAsync(i => i.Id == notification.Invitation.Id, ct);

        if (invitationToDelete is not null)
            invitations.Remove(invitationToDelete);

        await notifications.SaveChangesAsync(ct);

        // 3. Notify the invitee that invitation was cancelled
        DomainNotification domainNotification = DomainNotification.Create(
            userId: notification.Invitation.InviteeUserId,
            title: "🚫 Invitation cancelled",
            body: $"{inviterInfo} cancelled the invitation to {familyInfo}",
            type: NotificationType.InvitationCancelled,
            actorUserId: notification.Invitation.InviterUserId,
            data: new Dictionary<string, string>
            {
                [NotificationDataKeys.FAMILY_ID] = notification.Invitation.FamilyId.ToString(),
                [NotificationDataKeys.INVITER_USER_ID] = notification.Invitation.InviterUserId.ToString(),
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
