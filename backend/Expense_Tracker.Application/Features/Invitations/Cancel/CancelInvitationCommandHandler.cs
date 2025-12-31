using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Invitation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Invitations.Cancel;

public sealed class CancelInvitationCommandHandler(IAppDbContext db)
    : IRequestHandler<CancelInvitationCommand, Result>
{
    public async Task<Result> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get invitation
        Invitation? invitation = await db.Invitations
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(
                DomainError.NotFound(nameof(Invitation)));



        // 3. Cancel invitation (domain logic validates inviter and fires event)
        Result cancelResult = invitation.Cancel(request.RequesterId);
        if (cancelResult.IsFailure)
            return cancelResult;

        // 3. Save changes (event handler will delete invitation and notifications)
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}