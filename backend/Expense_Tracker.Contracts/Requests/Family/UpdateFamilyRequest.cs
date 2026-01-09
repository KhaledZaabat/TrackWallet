namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record UpdateFamilyRequest(
    string? Name,
    string? FamilyBio
);
