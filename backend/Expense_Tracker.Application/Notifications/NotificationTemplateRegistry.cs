using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Application.Notifications;

/// <summary>
/// Default implementation of <see cref="INotificationTemplateRegistry"/>.
/// Maps each strongly-typed payload to its presentation. The strings live here
/// instead of in event handlers so a copy change touches exactly one file.
/// </summary>
public sealed class NotificationTemplateRegistry : INotificationTemplateRegistry
{
    public NotificationTemplate Render(NotificationPayload payload) => payload switch
    {
        FamilyInvitationPayload p => new NotificationTemplate(
            Title: "New family invitation",
            Body: $"{p.InviterUserName} invited you to join {p.FamilyName}.",
            IconKey: NotificationIcons.FamilyInvitation,
            Category: NotificationCategory.Family,
            Severity: NotificationSeverity.Info,
            ResourceUri: $"/invitations/{p.InvitationId}"),

        InvitationAcceptedPayload p => new NotificationTemplate(
            Title: "Invitation accepted",
            Body: $"{p.InviteeUserName} accepted your invitation to {p.FamilyName}.",
            IconKey: NotificationIcons.InvitationAccepted,
            Category: NotificationCategory.Family,
            Severity: NotificationSeverity.Success,
            ResourceUri: $"/families/{p.FamilyId}"),

        InvitationDeclinedPayload p => new NotificationTemplate(
            Title: "Invitation declined",
            Body: $"{p.InviteeUserName} declined your invitation to {p.FamilyName}.",
            IconKey: NotificationIcons.InvitationDeclined,
            Category: NotificationCategory.Family,
            Severity: NotificationSeverity.Warning,
            ResourceUri: null),

        InvitationCancelledPayload p => new NotificationTemplate(
            Title: "Invitation cancelled",
            Body: $"{p.InviterUserName} cancelled the invitation to {p.FamilyName}.",
            IconKey: NotificationIcons.InvitationCancelled,
            Category: NotificationCategory.Family,
            Severity: NotificationSeverity.Warning,
            ResourceUri: null),

        TransactionCreatedPayload p => new NotificationTemplate(
            Title: p.TransactionType.Equals("Expense", StringComparison.OrdinalIgnoreCase)
                ? "New expense added"
                : "New income added",
            Body: $"{p.CreatorUserName} added {p.Amount:0.##} in {p.FamilyName}.",
            IconKey: p.TransactionType.Equals("Expense", StringComparison.OrdinalIgnoreCase)
                ? NotificationIcons.Expense
                : NotificationIcons.Income,
            Category: NotificationCategory.Activity,
            Severity: NotificationSeverity.Info,
            ResourceUri: $"/families/{p.FamilyId}/transactions/{p.TransactionId}"),

        _ => throw new InvalidOperationException(
            $"No notification template registered for payload type '{payload.GetType().Name}'."),
    };
}

/// <summary>
/// Stable icon keys the SPA maps to concrete assets. Keeping them as constants
/// stops typos at compile time.
/// </summary>
public static class NotificationIcons
{
    public const string FamilyInvitation = "family-invitation";
    public const string InvitationAccepted = "invitation-accepted";
    public const string InvitationDeclined = "invitation-declined";
    public const string InvitationCancelled = "invitation-cancelled";
    public const string Expense = "expense";
    public const string Income = "income";
}
