using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Invitation.Enums;
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
        var query =
            from i in db.Invitations
            join inviter in db.Users on i.InviterUserId equals inviter.Id
            join family in db.Families on i.FamilyId equals family.Id
            where i.InviteeUserId == request.UserId
            select new { i, inviter, family };


        if (request.Status.HasValue)
        {
            query = query.Where(x => x.i.Status == request.Status.Value);
        }
        else
        {
            query = query.Where(x => x.i.Status == InvitationStatus.Pending);
        }

        var result = await query
            .OrderByDescending(x => x.i.SentAtUtc)
            .Select(x => new InvitationResponse(
                x.i.Id,
                x.i.InviteeUserId,
                x.i.InviterUserId,
                x.i.FamilyId,
                x.i.IsParent,
                x.i.Status,
                x.i.SentAtUtc,
                x.inviter.FullName,
                x.family.Name
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(result);
    }
}