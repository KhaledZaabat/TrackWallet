namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record FamilyResponse(
    Guid Id,
    string Name,
    decimal CurrentBudget,
    string? FamilyBio,
    List<FamilyMemberProfile> Members
);
