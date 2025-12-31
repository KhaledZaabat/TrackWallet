using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
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
        // 1. Verify user is a parent member of the family
        bool isParent = await db.FamilyUsers
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.UserId &&
                fu.IsParent,
                cancellationToken);

        if (!isParent)
            return Result.Failure<List<InvitationResponse>>(
                DomainError.Forbidden("Only parent members can view sent invitations."));

        // 2. Get all invitations sent from this family
        var invitations = await db.Invitations
            .Include(i => i.Family)
            .Where(i => i.FamilyId == request.FamilyId)
            .OrderByDescending(i => i.SentAtUtc)
            .ToListAsync(cancellationToken);

        var response = invitations.Adapt<List<InvitationResponse>>();
        return Result.Success(response);
    }
}

