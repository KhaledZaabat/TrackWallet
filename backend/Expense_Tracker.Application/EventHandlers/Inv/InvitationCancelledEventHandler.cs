using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Notifications;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCancelledEventHandler(
    IRepository<global::Expense_Tracker.Domain.Users.User> users,
    IRepository<Family> families,
    IRepository<DomainNotification> notifications,
    IRepository<Invitation> invitations,
    IUnifiedNotificationDispatcher dispatcher,
    INotificationBuilder notificationBuilder,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    public async Task Handle(InvitationCancelledEvent e, CancellationToken ct)
    {
        string? inviterName = await users.Query()
            .Where(u => u.Id == e.Invitation.InviterUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviteeInfo = await users.Query()
            .Where(u => u.Id == e.Invitation.InviteeUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
            .SingleOrDefaultAsync(ct);

        string? familyName = await families.Query()
            .Where(f => f.Id == e.Invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        if (inviteeInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviteeInfo.Email))
        {
            await SendCancelledEmailAsync(
                inviteeInfo.Email,
                inviteeInfo.UserName ?? string.Empty,
                inviterName ?? string.Empty,
                familyName ?? string.Empty,
                ct);
        }

        // Clean up the prior "you have an invitation" notification(s) and the
        // invitation row itself before posting the cancellation.
        var relatedNotifications = await notifications.QueryTracked()
            .Where(n => n.UserId == e.Invitation.InviteeUserId
                     && n.ActorUserId == e.Invitation.InviterUserId
                     && n.Type == Domain.PushNotifications.Enums.NotificationType.FamilyInvitation)
            .ToListAsync(ct);

        if (relatedNotifications.Count > 0)
            notifications.RemoveRange(relatedNotifications);

        Invitation? invitationToDelete = await invitations.QueryTracked()
            .FirstOrDefaultAsync(i => i.Id == e.Invitation.Id, ct);

        if (invitationToDelete is not null)
            invitations.Remove(invitationToDelete);

        await notifications.SaveChangesAsync(ct);

        DomainNotification domainNotification = notificationBuilder.Build(
            recipientUserId: e.Invitation.InviteeUserId,
            actorUserId: e.Invitation.InviterUserId,
            payload: new InvitationCancelledPayload(
                FamilyId: e.Invitation.FamilyId,
                FamilyName: familyName ?? string.Empty,
                InviterUserId: e.Invitation.InviterUserId,
                InviterUserName: inviterName ?? string.Empty));

        await dispatcher.EnqueueAsync(domainNotification, ct);
    }

    private async Task SendCancelledEmailAsync(
        string email,
        string inviteeName,
        string inviterName,
        string familyName,
        CancellationToken ct)
    {
        string template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationCancelledTemplate, ct);

        string body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["InviteeName"] = inviteeName,
            ["InviterName"] = inviterName,
            ["FamilyName"] = familyName,
            ["AppLink"] = "/invitations",
        });

        await notification.SendEmailAsync(
            to: email,
            subject: $"Invitation cancelled - {familyName}",
            htmBody: body,
            cancellationToken: ct);
    }
}
