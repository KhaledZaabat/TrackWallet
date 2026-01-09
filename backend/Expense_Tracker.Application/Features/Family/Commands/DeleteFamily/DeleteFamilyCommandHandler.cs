using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Invitation.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Family.Commands.DeleteFamily;

public sealed class DeleteFamilyCommandHandler(
    IAppDbContext db
) : IRequestHandler<DeleteFamilyCommand, Result>
{
    public async Task<Result> Handle(
        DeleteFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify requester
        var requestingUser = await db.FamilyUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId &&
                      fu.UserId == request.RequestingUserId,
                cancellationToken);

        if (requestingUser is null)
            return Result.Failure(
                DomainError.NotFound(nameof(FamilyUser),
                "You are not a member of this family."));

        if (!requestingUser.IsParent)
            return Result.Failure(
                DomainError.Forbidden(nameof(FamilyUser),
                "Only parents can delete the family."));

        // 2. Ensure family exists
        var familyExists = await db.Families
            .AnyAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (!familyExists)
            return Result.Failure(
                DomainError.NotFound(nameof(Domain.FamilyFolder.Family)));

        // 3. Cancel pending invitations 
        var pendingInvitations = await db.Invitations
            .Where(i => i.FamilyId == request.FamilyId &&
                        i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var invitation in pendingInvitations)
        {
            invitation.Cancel(request.RequestingUserId);
        }

        // 4. Save → domain events are dispatched safely
        await db.SaveChangesAsync(cancellationToken);

        // 5. Hard delete family-user links
        await db.FamilyUsers
            .Where(fu => fu.FamilyId == request.FamilyId)
            .ExecuteDeleteAsync(cancellationToken);

        // 6. Hard delete family
        await db.Families
            .Where(f => f.Id == request.FamilyId)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}