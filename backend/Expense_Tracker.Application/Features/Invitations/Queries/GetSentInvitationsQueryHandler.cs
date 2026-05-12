using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Application.Interfaces;
using ErrorOr;
using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Invitations.Queries;

public sealed class GetSentInvitationsQueryHandler(
    IRepository<FamilyUser> familyUserRepo,
    IRepository<Invitation> invitationRepo,
    IRepository<User> userRepo,
    IRepository<global::Expense_Tracker.Domain.FamilyFolder.Family> familyRepo)
{
    public async Task<ErrorOr<List<InvitationResponse>>> Handle(
        GetSentInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Verify parent
        bool isParent = await familyUserRepo.QueryTracked()
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.UserId &&
                fu.IsParent,
                cancellationToken);

        if (!isParent)
            return DomainErrors.GeneralErrors.Forbidden("Only parent members can view sent invitations.");

        // 2. Query
        var query =
            from i in invitationRepo.QueryTracked()
            join inviter in userRepo.QueryTracked() on i.InviterUserId equals inviter.Id
            join invitee in userRepo.QueryTracked() on i.InviteeUserId equals invitee.Id
            join family in familyRepo.QueryTracked() on i.FamilyId equals family.Id
            where i.FamilyId == request.FamilyId
            select new { i, inviter, invitee, family };

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

        return result;
    }
}
