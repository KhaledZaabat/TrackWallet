using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Invitations.Decline;

public sealed record DeclineInvitationCommand(
    Guid InvitationId,
    Guid UserId) : IRequest<Result>;
