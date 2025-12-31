using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Invitations.Cancel;

public sealed record CancelInvitationCommand(
    Guid InvitationId,
    Guid RequesterId) : IRequest<Result>;
