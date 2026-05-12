using Expense_Tracker.Application.Interfaces;
using ErrorOr;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Domain.Invitation;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Invitations.Cancel;

public sealed class CancelInvitationCommandHandler(
    IRepository<Invitation> invitationRepo,
    IMessageBus bus)
{
    public async Task<ErrorOr<Success>> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get invitation
        Invitation? invitation = await invitationRepo.QueryTracked()
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Invitation));

        // 2. Cancel invitation (domain logic validates inviter)
        ErrorOr<Success> cancelResult = invitation.Cancel(request.RequesterId);
        if (cancelResult.IsError)
            return cancelResult.Errors;

        // 3. Save changes (event handler will delete invitation and notifications)
        await invitationRepo.SaveChangesAsync(cancellationToken);

        // 4. Publish event
        await bus.PublishAsync(new InvitationCancelledEvent(invitation));

        return new Success();
    }
}
