using Expense_Tracker.Contracts.Reponses.Inv;

namespace Expense_Tracker.Application.Features.Invitations.Send;

public sealed record SendInvitationCommand(
    string InviteeEmail,
    bool IsParent,
    Guid InviterUserId,
    Guid FamilyId
);
