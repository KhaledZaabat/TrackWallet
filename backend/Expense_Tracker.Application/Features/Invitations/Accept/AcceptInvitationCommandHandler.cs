using Expense_Tracker.Domain.Users;
using Expense_Tracker.Application.Interfaces;
using ErrorOr;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Invitation;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Invitations.Accept;

public sealed class AcceptInvitationCommandHandler(
    IRepository<Invitation> invitationRepo,
    IRepository<FamilyUser> familyUserRepo,
    IMessageBus bus)
{
    public async Task<ErrorOr<Success>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        Invitation? invitation = await invitationRepo.QueryTracked()
            .Include(i => i.Family)
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Invitation));

        if (invitation.InviteeUserId != request.UserId)
            return DomainErrors.GeneralErrors.Forbidden("You can only accept invitations sent to you.");

        ErrorOr<Success> acceptResult = invitation.Accept();
        if (acceptResult.IsError)
            return acceptResult.Errors;

        ErrorOr<FamilyUser> familyUserResult = FamilyUser.Create(
            invitation.FamilyId,
            invitation.InviteeUserId,
            invitation.IsParent,
            invitation.InviterUserId);

        if (familyUserResult.IsError)
            return familyUserResult.Errors;

        FamilyUser familyUser = familyUserResult.Value;
        await familyUserRepo.AddAsync(familyUser);

        await invitationRepo.SaveChangesAsync(cancellationToken);

        await bus.PublishAsync(new InvitationAcceptedEvent(invitation));

        return new Success();
    }
}
