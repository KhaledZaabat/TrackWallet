namespace Expense_Tracker.Application.Features.Family.Commands.LeaveFamily;

public sealed record LeaveFamilyCommand(
    Guid UserId,
    Guid FamilyId
);
