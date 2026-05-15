using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Application.Notifications;

public sealed class NotificationBuilder(INotificationTemplateRegistry registry)
    : INotificationBuilder
{
    public DomainNotification Build(
        Guid recipientUserId,
        Guid? actorUserId,
        NotificationPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        NotificationTemplate t = registry.Render(payload);
        NotificationType type = ResolveType(payload);

        return DomainNotification.Create(
            userId: recipientUserId,
            actorUserId: actorUserId,
            type: type,
            category: t.Category,
            severity: t.Severity,
            title: t.Title,
            body: t.Body,
            iconKey: t.IconKey,
            resourceUri: t.ResourceUri,
            payload: payload);
    }

    /// <summary>
    /// Maps a payload subtype to the canonical <see cref="NotificationType"/>
    /// value. Centralised here so the enum is the only place that knows about
    /// the discriminator.
    /// </summary>
    private static NotificationType ResolveType(NotificationPayload payload) => payload switch
    {
        FamilyInvitationPayload => NotificationType.FamilyInvitation,
        InvitationAcceptedPayload => NotificationType.InvitationAccepted,
        InvitationDeclinedPayload => NotificationType.InvitationDeclined,
        InvitationCancelledPayload => NotificationType.InvitationCancelled,
        TransactionCreatedPayload => NotificationType.TransactionCreated,
        _ => throw new InvalidOperationException(
            $"No NotificationType mapped for payload '{payload.GetType().Name}'."),
    };
}
