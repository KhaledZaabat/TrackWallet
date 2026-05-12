namespace Expense_Tracker.Application.Features.Family.Commands.DeleteFamily;

public sealed record DeleteFamilyCommand(
    Guid FamilyId,
    Guid RequestingUserId
);
