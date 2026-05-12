namespace Expense_Tracker.Application.Features.Family.Commands.UpdateFamily;

public sealed record UpdateFamilyCommand(
    Guid FamilyId,
    string? Name,
    string? FamilyBio
);
