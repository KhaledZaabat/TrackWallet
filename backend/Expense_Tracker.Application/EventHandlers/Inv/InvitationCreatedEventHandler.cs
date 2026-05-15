using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Notifications;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCreatedEventHandler(
    IRepository<global::Expense_Tracker.Domain.Users.User> users,
    IRepository<Family> families,
    IUnifiedNotificationDispatcher dispatcher,
    INotificationBuilder notificationBuilder,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    public async Task Handle(InvitationCreatedEvent e, CancellationToken ct)
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

        DomainNotification domainNotification = notificationBuilder.Build(
            recipientUserId: e.Invitation.InviteeUserId,
            actorUserId: e.Invitation.InviterUserId,
            payload: new FamilyInvitationPayload(
                InvitationId: e.Invitation.Id,
                FamilyId: e.Invitation.FamilyId,
                FamilyName: familyName ?? string.Empty,
                InviterUserId: e.Invitation.InviterUserId,
                InviterUserName: inviterName ?? string.Empty));

        await dispatcher.EnqueueAsync(domainNotification, ct);

        if (inviteeInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviteeInfo.Email))
        {
            await SendInvitationEmailAsync(
                inviteeInfo.Email,
                inviteeInfo.UserName ?? string.Empty,
                inviterName ?? string.Empty,
                familyName ?? string.Empty,
                ct);
        }
    }

    private async Task SendInvitationEmailAsync(
        string email,
        string inviteeName,
        string inviterName,
        string familyName,
        CancellationToken ct)
    {
        string template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationCreatedTemplate, ct);

        string body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["InviteeName"] = inviteeName,
            ["InviterName"] = inviterName,
            ["FamilyName"] = familyName,
            ["AppLink"] = "/invitations",
        });

        await notification.SendEmailAsync(
            to: email,
            subject: $"{inviterName} invited you to join {familyName}",
            htmBody: body,
            cancellationToken: ct);
    }
}
