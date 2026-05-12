using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.Invitation.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Commands.DeleteFamily;

public sealed class DeleteFamilyCommandHandler(
    IRepository<FamilyUser> familyUsers,
    IRepository<global::Expense_Tracker.Domain.FamilyFolder.Family> families,
    IRepository<Invitation> invitations
)
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify requester
        var requestingUser = await familyUsers.Query()
            .FirstOrDefaultAsync(
                fu => fu.FamilyId == request.FamilyId &&
                      fu.UserId == request.RequestingUserId,
                cancellationToken);

        if (requestingUser is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(FamilyUser),
                "You are not a member of this family.");

        if (!requestingUser.IsParent)
            return DomainErrors.GeneralErrors.Forbidden(
                "Only parents can delete the family.");

        // 2. Ensure family exists
        var familyExists = await families.QueryTracked()
            .AnyAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (!familyExists)
            return DomainErrors.GeneralErrors.NotFound(nameof(Family));

        // 3. Cancel pending invitations 
        var pendingInvitations = await invitations.QueryTracked()
            .Where(i => i.FamilyId == request.FamilyId &&
                        i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var invitation in pendingInvitations)
        {
            invitation.Cancel(request.RequestingUserId);
        }

        // 4. Save → domain events are dispatched safely
        await invitations.SaveChangesAsync(cancellationToken);

        // 5. Hard delete family-user links
        await familyUsers.Query()
            .Where(fu => fu.FamilyId == request.FamilyId)
            .ExecuteDeleteAsync(cancellationToken);

        // 6. Hard delete family
        await families.Query()
            .Where(f => f.Id == request.FamilyId)
            .ExecuteDeleteAsync(cancellationToken);

        return new Success();
    }
}
