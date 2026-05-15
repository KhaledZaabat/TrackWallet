namespace Expense_Tracker.Domain.PushNotifications.Enums;

/// <summary>
/// Stable identifiers for the kinds of notifications this product emits. The string
/// form (via <see cref="System.Enum"/>) is persisted to the <c>Notifications.Type</c>
/// column and is also the polymorphic discriminator for the JSON payload, so adding
/// a new value here is safe but renaming an existing one is a breaking change.
/// </summary>
public enum NotificationType
{
    // Invitations
    FamilyInvitation,
    InvitationAccepted,
    InvitationDeclined,
    InvitationCancelled,

    // Money
    TransactionCreated,
}
