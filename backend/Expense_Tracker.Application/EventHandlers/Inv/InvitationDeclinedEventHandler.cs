using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Notifications;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationDeclinedEventHandler(
    IRepository<global::Expense_Tracker.Domain.Users.User> users,
    IRepository<Family> families,
    IUnifiedNotificationDispatcher dispatcher,
    INotificationBuilder notificationBuilder,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    public async Task Handle(InvitationDeclinedEvent e, CancellationToken ct)
    {
        string? inviteeName = await users.Query()
            .Where(u => u.Id == e.Invitation.InviteeUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviterInfo = await users.Query()
            .Where(u => u.Id == e.Invitation.InviterUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
            .SingleOrDefaultAsync(ct);

        string? familyName = await families.Query()
            .Where(f => f.Id == e.Invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        DomainNotification domainNotification = notificationBuilder.Build(
            recipientUserId: e.Invitation.InviterUserId,
            actorUserId: e.Invitation.InviteeUserId,
            payload: new InvitationDeclinedPayload(
                InvitationId: e.Invitation.Id,
                FamilyId: e.Invitation.FamilyId,
                FamilyName: familyName ?? string.Empty,
                InviteeUserId: e.Invitation.InviteeUserId,
                InviteeUserName: inviteeName ?? string.Empty));

        await dispatcher.EnqueueAsync(domainNotification, ct);

        if (inviterInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviterInfo.Email))
        {
            await SendDeclinedEmailAsync(
                inviterInfo.Email,
                inviterInfo.UserName ?? string.Empty,
                inviteeName ?? string.Empty,
                familyName ?? string.Empty,
                ct);
        }
    }

    private async Task SendDeclinedEmailAsync(
        string email,
        string inviterName,
        string inviteeName,
        string familyName,
        CancellationToken ct)
    {
        string template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationDeclinedTemplate, ct);

        string body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["InviterName"] = inviterName,
            ["InviteeName"] = inviteeName,
            ["FamilyName"] = familyName,
            ["AppLink"] = "/invitations",
        });

        await notification.SendEmailAsync(
            to: email,
            subject: $"{inviteeName} declined your invitation",
            htmBody: body,
            cancellationToken: ct);
    }
}
