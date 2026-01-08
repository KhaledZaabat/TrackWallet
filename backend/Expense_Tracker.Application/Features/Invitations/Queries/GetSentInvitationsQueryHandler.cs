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
            join invitee in db.Users on i.InviteeUserId equals invitee.Id  // ✅ Added invitee join
            join family in db.Families on i.FamilyId equals family.Id
            where i.FamilyId == request.FamilyId
            select new { i, inviter, invitee, family };  // ✅ Added invitee to select

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.i.Status == request.Status.Value);
        }

        var result = await query
            .OrderByDescending(x => x.i.SentAtUtc)
            .Select(x => new InvitationResponse(
                InvitationId: x.i.Id,
                InviteeUserId: x.i.InviteeUserId,
                InviteeEmail: x.invitee.Email,
                InviterUserId: x.i.InviterUserId,
                InviterEmail: x.inviter.Email,
                FamilyId: x.i.FamilyId,
                IsParent: x.i.IsParent,
                Status: x.i.Status,
                SentAtUtc: x.i.SentAtUtc,
                InviterName: x.inviter.FullName,
                FamilyName: x.family.Name
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(result);
    }
}