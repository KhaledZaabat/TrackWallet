using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Invitations.Accept;

public sealed record AcceptInvitationCommand(
    Guid InvitationId,
    Guid UserId) : IRequest<Result>;
