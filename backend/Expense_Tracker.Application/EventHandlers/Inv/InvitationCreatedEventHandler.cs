using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class InvitationCreatedEventHandler(
    IAppDbContext context,
    IUnifiedNotificationDispatcher dispatcher,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
    : INotificationHandler<InvitationCreatedEvent>
{
    public async Task Handle(
        InvitationCreatedEvent notification,
        CancellationToken ct)
    {
        // Get inviter, invitee, and family information
        var inviterInfo = await context.Users
            .Where(u => u.Id == notification.Invitation.InviterUserId)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        var inviteeInfo = await context.Users
            .Where(u => u.Id == notification.Invitation.InviteeUserId)
            .Select(u => new { u.UserName, u.Email, u.NotificationPreferences.EmailNotifications })
            .SingleOrDefaultAsync(ct);

        var familyInfo = await context.Families
            .Where(f => f.Id == notification.Invitation.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        // Send push notification
        DomainNotification domainNotification = DomainNotification.Create(
            userId: notification.Invitation.InviteeUserId,
            title: "👨‍👩‍👧‍👦 New family invitation",
            body: $"{inviterInfo} invited you to join {familyInfo} Family",
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

        // Send email if enabled
        if (inviteeInfo?.EmailNotifications == true && !string.IsNullOrWhiteSpace(inviteeInfo.Email))
        {
            await SendInvitationEmailAsync(
                inviteeInfo.Email,
                inviteeInfo.UserName,
                inviterInfo,
                familyInfo,
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
        var template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.InvitationCreatedTemplate,
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
            subject: $"🎉 {inviterName} invited you to join {familyName}",
            htmBody: body,
            cancellationToken: ct);
    }
}