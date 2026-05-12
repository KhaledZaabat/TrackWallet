using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.FamilyUserFolder;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Commands.KickFamilyMember;

public sealed class KickFamilyMemberCommandHandler(
    IRepository<FamilyUser> familyUsers
)
{
    public async Task<ErrorOr<Success>> Handle(
        KickFamilyMemberCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify the requesting user is a parent in the family
        var requestingUser = await familyUsers.Query()
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId && fu.UserId == request.RequestingUserId,
                cancellationToken);

        if (requestingUser is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(FamilyUser), "You are not a member of this family.");

        if (!requestingUser.IsParent)
            return DomainErrors.GeneralErrors.Forbidden(
                "Only parents can kick members from the family.");

        // 2. Find the target user's membership
        var targetFamilyUser = await familyUsers.QueryTracked()
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserIdToKick,
                cancellationToken);

        if (targetFamilyUser is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(FamilyUser), "The specified user is not a member of this family.");

        // 3. Cannot kick another parent
        if (targetFamilyUser.IsParent)
            return DomainErrors.GeneralErrors.BusinessRule(
                nameof(FamilyUser),
                "You cannot kick another parent. They must be demoted first or leave voluntarily.");

        // 4. Remove the family user record
        // Note: Transactions remain untouched - they stay with the family for budget history
        familyUsers.Remove(targetFamilyUser);

        // 5. Save changes
        await familyUsers.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
