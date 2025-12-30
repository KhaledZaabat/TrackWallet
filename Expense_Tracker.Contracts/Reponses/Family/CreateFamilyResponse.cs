namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record CreateFamilyResponse(
    Guid FamilyId,
    string Name,
    decimal CurrentBudget,
    string? FamilyBio,
    DateTimeOffset CreatedAtUtc,
    bool IsParent,
    int MemberCount
);