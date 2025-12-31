using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Invitation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Invitations.Decline;

public sealed class DeclineInvitationCommandHandler(IAppDbContext db)
    : IRequestHandler<DeclineInvitationCommand, Result>
{
    public async Task<Result> Handle(
        DeclineInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get invitation
        Invitation? invitation = await db.Invitations
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(
                DomainError.NotFound(nameof(Invitation)));

        // 2. Verify user is the invitee
        if (invitation.InviteeUserId != request.UserId)
            return Result.Failure(
                DomainError.Forbidden("You can only decline invitations sent to you."));

        // 3. Decline invitation (domain logic with event)
        Result declineResult = invitation.Decline();
        if (declineResult.IsFailure)
            return declineResult;

        // 4. Save changes
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}