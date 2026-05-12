using Expense_Tracker.Contracts.Reponses.Family;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

public sealed record SelectFamilyCommand(
    Guid UserId,
    Guid FamilyId,
    string DeviceId
);
