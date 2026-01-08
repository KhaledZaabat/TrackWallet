using Expense_Tracker.Domain.Invitation.Enums;

namespace Expense_Tracker.Contracts.Reponses.Inv;

public sealed record InvitationResponse(
    Guid InvitationId,
    Guid InviteeUserId,
    string InviteeEmail,
    Guid InviterUserId,
    string InviterEmail,
    Guid FamilyId,
    bool IsParent,
    InvitationStatus Status,
    DateTimeOffset SentAtUtc,
    string InviterName,
    string FamilyName
);
