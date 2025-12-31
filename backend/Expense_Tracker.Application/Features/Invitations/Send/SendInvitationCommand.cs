using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Invitations.Send;

public sealed record SendInvitationCommand(
    string InviteeEmail,
    bool IsParent,
    Guid InviterUserId,
    Guid FamilyId
) : IRequest<Result<InvitationResponse>>;
