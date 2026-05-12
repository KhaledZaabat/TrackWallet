namespace Expense_Tracker.Application.Features.Invitations.Accept;

public sealed record AcceptInvitationCommand(
    Guid InvitationId,
    Guid UserId);
