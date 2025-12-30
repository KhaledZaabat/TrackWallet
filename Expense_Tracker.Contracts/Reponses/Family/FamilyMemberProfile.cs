namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record FamilyMemberProfile(
    Guid UserId,
    string FullName,
    string? ProfileImageUrl,
    bool IsParent
);