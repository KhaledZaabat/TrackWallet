namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record FamilyUserSimpleResponse(
    Guid UserId,
    string FullName
);