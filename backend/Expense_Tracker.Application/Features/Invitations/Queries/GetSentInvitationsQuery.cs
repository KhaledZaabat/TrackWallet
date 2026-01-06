using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Invitation.Enums;
using MediatR;

namespace Expense_Tracker.Application.Features.Invitations.Queries;

public sealed record GetSentInvitationsQuery(
    Guid FamilyId,
    Guid UserId,
    InvitationStatus? Status = null)
    : IRequest<Result<List<InvitationResponse>>>;