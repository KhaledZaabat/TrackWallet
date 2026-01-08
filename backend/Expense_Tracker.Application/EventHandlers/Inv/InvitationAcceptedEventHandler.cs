using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationAcceptedEventHandler(
    IAppDbContext context,
    IUnifiedNotificationDispatcher dispatcher,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
    : INotificationHandler<InvitationAcceptedEvent>
{
    public async Task Handle(
        InvitationAcceptedEvent notification,
        CancellationToken ct)
    {
        // Get invitee, inviter, and family information
        var inviteeInfo = await context.Users
            .Where(u => u.Id == notification.Invitation.InviteeUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviterInfo = await context.Users
            .Where(u => u.Id == notification.Invitation.InviterUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
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

        // Send email if enabled
        if (inviterInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviterInfo.Email))
        {
            await SendAcceptedEmailAsync(
                inviterInfo.Email,
                inviterInfo.UserName,
                inviteeInfo,
                familyInfo,
                ct);
        }
    }

    private async Task SendAcceptedEmailAsync(
        string email,
        string inviterName,
        string inviteeName,
        string familyName,
        CancellationToken ct)
    {
        var template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationAcceptedTemplate,
            ct);

        var body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["InviterName"] = inviterName,
            ["InviteeName"] = inviteeName,
            ["FamilyName"] = familyName,
            ["AppLink"] = "expensetracker://family"
        });

        await notification.SendEmailAsync(
            to: email,
            subject: $"✅ {inviteeName} accepted your invitation!",
            htmBody: body,
            cancellationToken: ct);
    }
}
