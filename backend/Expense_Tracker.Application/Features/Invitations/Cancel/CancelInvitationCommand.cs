namespace Expense_Tracker.Application.Features.Invitations.Cancel;

public sealed record CancelInvitationCommand(
    Guid InvitationId,
    Guid RequesterId);
