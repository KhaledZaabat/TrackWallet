using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Invitation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Invitations.Accept;

public sealed class AcceptInvitationCommandHandler(IAppDbContext db)
    : IRequestHandler<AcceptInvitationCommand, Result>
{
    public async Task<Result> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get invitation
        Invitation? invitation = await db.Invitations
            .Include(i => i.Family)
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
            return Result.Failure(
                DomainError.NotFound(nameof(Invitation)));

        // 2. Verify user is the invitee
        if (invitation.InviteeUserId != request.UserId)
            return Result.Failure(
                DomainError.Forbidden("You can only accept invitations sent to you."));

        // 3. Accept invitation (domain logic)
        Result acceptResult = invitation.Accept();
        if (acceptResult.IsFailure)
            return acceptResult;

        // 4. Add user to family
        Result<FamilyUser> familyUserResult = FamilyUser.Create(
            invitation.FamilyId,
            invitation.InviteeUserId,
            invitation.IsParent,
            invitation.InviterUserId);

        if (familyUserResult.IsFailure)
            return Result.Failure(familyUserResult.TryGetError());

        FamilyUser familyUser = familyUserResult.TryGetValue();
        db.FamilyUsers.Add(familyUser);

        // 5. Save changes
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}