using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyUserFolder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Family.Commands.LeaveFamily;

public sealed class LeaveFamilyCommandHandler(
    IAppDbContext db
) : IRequestHandler<LeaveFamilyCommand, Result>
{
    public async Task<Result> Handle(
        LeaveFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find the user's membership in the family
        var familyUser = await db.FamilyUsers
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserId,
                cancellationToken);

        if (familyUser is null)
            return Result.Failure(
                DomainError.NotFound(nameof(FamilyUser), "You are not a member of this family."));

        // 2. If user is a parent, check if there are other parents
        if (familyUser.IsParent)
        {
            var otherParentsCount = await db.FamilyUsers
                .CountAsync(
                    fu => fu.FamilyId == request.FamilyId
                          && fu.IsParent
                          && fu.UserId != request.UserId,
                    cancellationToken);

            if (otherParentsCount == 0)
            {
                // Check if there are other members who could be promoted
                var otherMembersExist = await db.FamilyUsers
                    .AnyAsync(
                        fu => fu.FamilyId == request.FamilyId && fu.UserId != request.UserId,
                        cancellationToken);

                if (otherMembersExist)
                {
                    return Result.Failure(
                        DomainError.BusinessRule(
                            nameof(FamilyUser),
                            "You are the last parent. Please promote another member to parent before leaving, or delete the family."));
                }

                // If no other members exist, delete the family after removing the user
                //  Remove the FamilyUser FIRST, then delete the Family
                db.FamilyUsers.Remove(familyUser);

                await db.Families
                    .Where(f => f.Id == request.FamilyId && !f.IsDeleted)
                    .ExecuteDeleteAsync(cancellationToken);

                return Result.Success();
            }
        }

        // 3. Remove the family user record
        db.FamilyUsers.Remove(familyUser);

        // 4. Save changes
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}