using Expense_Tracker.Application.Interfaces;
using ErrorOr;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Domain.Invitation;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Invitations.Decline;

public sealed class DeclineInvitationCommandHandler(
    IRepository<Invitation> invitationRepo,
    IMessageBus bus)
{
    public async Task<ErrorOr<Success>> Handle(
        DeclineInvitationCommand request,
        CancellationToken cancellationToken)
    {
        Invitation? invitation = await invitationRepo.QueryTracked()
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Invitation));

        if (invitation.InviteeUserId != request.UserId)
            return DomainErrors.GeneralErrors.Forbidden("You can only decline invitations sent to you.");

        ErrorOr<Success> declineResult = invitation.Decline();
        if (declineResult.IsError)
            return declineResult.Errors;

        await invitationRepo.SaveChangesAsync(cancellationToken);

        await bus.PublishAsync(new InvitationDeclinedEvent(invitation));

        return new Success();
    }
}
