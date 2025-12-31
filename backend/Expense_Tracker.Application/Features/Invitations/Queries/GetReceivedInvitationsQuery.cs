using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Invitations.Queries;

public sealed record GetReceivedInvitationsQuery(Guid UserId) : IRequest<Result<List<InvitationResponse>>>;