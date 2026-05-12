using Expense_Tracker.Contracts.Reponses.Family;

namespace Expense_Tracker.Application.Features.Family.Commands.CreateFamily;

public sealed record CreateFamilyCommand(
    Guid UserId,
    string Name,
    decimal InitialBudget,
    string? FamilyBio
);
