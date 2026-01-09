namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record KickFamilyMemberRequest(
    Guid UserIdToKick
);
