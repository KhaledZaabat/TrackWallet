namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record FamilyContextDto(
    Guid FamilyId,
    string FamilyName,
    bool IsParent
);

