using User = Expense_Tracker.Domain.Users.User;
using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.PushNotifications.Enums;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationDeclinedEventHandler(
    IRepository<global::Expense_Tracker.Domain.Users.User> users,
    IRepository<Family> families,
    IUnifiedNotificationDispatcher dispatcher,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    public async Task Handle(
        InvitationDeclinedEvent notification,
        CancellationToken ct)
    {
        var inviteeInfo = await users.Query()
            .Where(u => u.Id == notification.Invitation.InviteeUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviterInfo = await users.Query()
            .Where(u => u.Id == notification.Invitation.InviterUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
            .SingleOrDefaultAsync(ct);

        var familyInfo = await families.Query()
            .Where(f => f.Id == notification.Invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        DomainNotification domainNotification = DomainNotification.Create(
            userId: notification.Invitation.InviterUserId,
            title: "❌ Invitation declined",
            body: $"{inviteeInfo} declined your invitation to {familyInfo}",
            type: NotificationType.InvitationDeclined,
            actorUserId: notification.Invitation.InviteeUserId,
            data: new Dictionary<string, string>
            {
                [NotificationDataKeys.INVITATION_ID] = notification.Invitation.Id.ToString(),
                [NotificationDataKeys.FAMILY_ID] = notification.Invitation.FamilyId.ToString(),
                [NotificationDataKeys.INVITEE_USER_ID] = notification.Invitation.InviteeUserId.ToString(),
                ["action"] = "none"
            });

        await dispatcher.EnqueueAsync(domainNotification, ct);

        if (inviterInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviterInfo.Email))
        {
            await SendDeclinedEmailAsync(
                inviterInfo.Email,
                inviterInfo.UserName,
                inviteeInfo,
                familyInfo,
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
        var template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationDeclinedTemplate,
            ct);

        var body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["InviterName"] = inviterName,
            ["InviteeName"] = inviteeName,
            ["FamilyName"] = familyName,
            ["AppLink"] = "expensetracker://invitations"
        });

        await notification.SendEmailAsync(
            to: email,
            subject: $"❌ {inviteeName} declined your invitation",
            htmBody: body,
            cancellationToken: ct);
    }
}
