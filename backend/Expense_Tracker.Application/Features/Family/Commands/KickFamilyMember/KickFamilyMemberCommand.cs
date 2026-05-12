namespace Expense_Tracker.Application.Features.Family.Commands.KickFamilyMember;

public sealed record KickFamilyMemberCommand(
    Guid FamilyId,
    Guid UserIdToKick,
    Guid RequestingUserId
);
