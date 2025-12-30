namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record CreateFamilyRequest(
    string Name,
    decimal InitialBudget,
    string? FamilyBio
);