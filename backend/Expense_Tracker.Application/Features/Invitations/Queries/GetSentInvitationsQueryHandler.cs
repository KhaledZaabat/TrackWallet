using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Invitations.Queries;

public sealed class GetSentInvitationsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetSentInvitationsQuery, Result<List<InvitationResponse>>>
{
    public async Task<Result<List<InvitationResponse>>> Handle(
        GetSentInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Verify parent
        bool isParent = await db.FamilyUsers
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.UserId &&
                fu.IsParent,
                cancellationToken);

        if (!isParent)
            return Result.Failure<List<InvitationResponse>>(
                DomainError.Forbidden("Only parent members can view sent invitations."));

        // 2. Query
        var query =
            from i in db.Invitations
            join inviter in db.Users on i.InviterUserId equals inviter.Id
            join family in db.Families on i.FamilyId equals family.Id
            where i.FamilyId == request.FamilyId
            select new { i, inviter, family };

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.i.Status == request.Status.Value);
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