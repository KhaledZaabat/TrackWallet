using Expense_Tracker.Domain.Invitation.Enums;

namespace Expense_Tracker.Contracts.Reponses.Inv;

public sealed record InvitationResponse(
    Guid InvitationId,
    Guid InviteeUserId,
    Guid InviterUserId,
    Guid FamilyId,
    bool IsParent,
    InvitationStatus Status,
    DateTimeOffset SentAtUtc,
    string InviterName,
    string FamilyName
);
