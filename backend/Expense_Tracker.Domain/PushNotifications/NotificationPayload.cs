using System.Text.Json.Serialization;

namespace Expense_Tracker.Domain.PushNotifications;


[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(FamilyInvitationPayload), typeDiscriminator: "family-invitation")]
[JsonDerivedType(typeof(InvitationAcceptedPayload), typeDiscriminator: "invitation-accepted")]
[JsonDerivedType(typeof(InvitationDeclinedPayload), typeDiscriminator: "invitation-declined")]
[JsonDerivedType(typeof(InvitationCancelledPayload), typeDiscriminator: "invitation-cancelled")]
[JsonDerivedType(typeof(TransactionCreatedPayload), typeDiscriminator: "transaction-created")]
public abstract record NotificationPayload;

public sealed record FamilyInvitationPayload(
    Guid InvitationId,
    Guid FamilyId,
    string FamilyName,
    Guid InviterUserId,
    string InviterUserName) : NotificationPayload;

public sealed record InvitationAcceptedPayload(
    Guid InvitationId,
    Guid FamilyId,
    string FamilyName,
    Guid InviteeUserId,
    string InviteeUserName) : NotificationPayload;

public sealed record InvitationDeclinedPayload(
    Guid InvitationId,
    Guid FamilyId,
    string FamilyName,
    Guid InviteeUserId,
    string InviteeUserName) : NotificationPayload;

public sealed record InvitationCancelledPayload(
    Guid FamilyId,
    string FamilyName,
    Guid InviterUserId,
    string InviterUserName) : NotificationPayload;

public sealed record TransactionCreatedPayload(
    Guid TransactionId,
    Guid FamilyId,
    string FamilyName,
    Guid CategoryId,
    decimal Amount,
    string TransactionType,
    Guid CreatorUserId,
    string CreatorUserName) : NotificationPayload;
