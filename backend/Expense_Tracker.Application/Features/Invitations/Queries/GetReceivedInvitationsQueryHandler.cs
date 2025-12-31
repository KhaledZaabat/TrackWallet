using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Invitation.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Invitations.Queries;

public sealed class GetReceivedInvitationsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetReceivedInvitationsQuery, Result<List<InvitationResponse>>>
{
    public async Task<Result<List<InvitationResponse>>> Handle(
        GetReceivedInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        var invitations = await db.Invitations
            .Include(i => i.Family)
            .Where(i => i.InviteeUserId == request.UserId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.SentAtUtc)
            .ToListAsync(cancellationToken);

        var response = invitations.Adapt<List<InvitationResponse>>();
        return Result.Success(response);
    }
}