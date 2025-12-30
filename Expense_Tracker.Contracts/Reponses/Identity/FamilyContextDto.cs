namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record FamilyContextDto(
    string FamilyId,
    string FamilyName,
    bool IsParent
);

