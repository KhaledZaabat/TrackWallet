namespace Expense_Tracker.Application.Features.Invitations.Decline;

public sealed record DeclineInvitationCommand(
    Guid InvitationId,
    Guid UserId);
