using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyUserFolder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Family.Commands.KickFamilyMember;

public sealed class KickFamilyMemberCommandHandler(
    IAppDbContext db
) : IRequestHandler<KickFamilyMemberCommand, Result>
{
    public async Task<Result> Handle(
        KickFamilyMemberCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify the requesting user is a parent in the family
        var requestingUser = await db.FamilyUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId && fu.UserId == request.RequestingUserId,
                cancellationToken);

        if (requestingUser is null)
            return Result.Failure(
                DomainError.NotFound(nameof(FamilyUser), "You are not a member of this family."));

        if (!requestingUser.IsParent)
            return Result.Failure(
                DomainError.Forbidden(
                    nameof(FamilyUser),
                    "Only parents can kick members from the family."));

        // 2. Find the target user's membership
        var targetFamilyUser = await db.FamilyUsers
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserIdToKick,
                cancellationToken);

        if (targetFamilyUser is null)
            return Result.Failure(
                DomainError.NotFound(nameof(FamilyUser), "The specified user is not a member of this family."));

        // 3. Cannot kick another parent
        if (targetFamilyUser.IsParent)
            return Result.Failure(
                DomainError.BusinessRule(
                    nameof(FamilyUser),
                    "You cannot kick another parent. They must be demoted first or leave voluntarily."));

        // 4. Remove the family user record
        // Note: Transactions remain untouched - they stay with the family for budget history
        db.FamilyUsers.Remove(targetFamilyUser);

        // 5. Save changes
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
