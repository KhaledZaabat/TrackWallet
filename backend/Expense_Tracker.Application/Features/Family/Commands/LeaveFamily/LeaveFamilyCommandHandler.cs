using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Commands.LeaveFamily;

public sealed class LeaveFamilyCommandHandler(
    IRepository<FamilyUser> familyUsers,
    IRepository<global::Expense_Tracker.Domain.FamilyFolder.Family> families
)
{
    public async Task<ErrorOr<Success>> Handle(
        LeaveFamilyCommand request,
        CancellationToken cancellationToken)
    {
        var familyUser = await familyUsers.QueryTracked()
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserId,
                cancellationToken);

        if (familyUser is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(FamilyUser), "You are not a member of this family.");

        if (familyUser.IsParent)
        {
            var otherParentsCount = await familyUsers.QueryTracked()
                .CountAsync(
                    fu => fu.FamilyId == request.FamilyId
                          && fu.IsParent
                          && fu.UserId != request.UserId,
                    cancellationToken);

            if (otherParentsCount == 0)
            {
                // Check if there are other members who could be promoted
                var otherMembersExist = await familyUsers.QueryTracked()
                    .AnyAsync(
                        fu => fu.FamilyId == request.FamilyId && fu.UserId != request.UserId,
                        cancellationToken);

                if (otherMembersExist)
                {
                    return DomainErrors.GeneralErrors.BusinessRule(
                        nameof(FamilyUser),
                        "You are the last parent. Please promote another member to parent before leaving, or delete the family.");
                }

                // If no other members exist, delete the family after removing the user
                //  Remove the FamilyUser FIRST, then delete the Family
                familyUsers.Remove(familyUser);

                await families.Query()
                    .Where(f => f.Id == request.FamilyId && !f.IsDeleted)
                    .ExecuteDeleteAsync(cancellationToken);

                return new Success();
            }
        }

        familyUsers.Remove(familyUser);

        await familyUsers.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
