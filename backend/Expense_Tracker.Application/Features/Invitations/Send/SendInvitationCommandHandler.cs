using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.Invitation.Enums;
using Expense_Tracker.Domain.Users;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Invitations.Send;

public sealed class SendInvitationCommandHandler(IAppDbContext db)
    : IRequestHandler<SendInvitationCommand, Result<InvitationResponse>>
{
    public async Task<Result<InvitationResponse>> Handle(
        SendInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify inviter is a parent member of the family
        bool isInviterParent = await db.FamilyUsers
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.InviterUserId &&
                fu.IsParent,
                cancellationToken);

        if (!isInviterParent)
            return Result.Failure<InvitationResponse>(
                DomainError.Forbidden("Only parent members can send invitations."));

        // 2. Find invitee by email
        User? invitee = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.InviteeEmail.Trim().ToLowerInvariant(),
                cancellationToken);

        if (invitee is null)
            return Result.Failure<InvitationResponse>(
                DomainError.NotFound(nameof(User)));

        // 3. Check if user is already a family member
        bool isAlreadyMember = await db.FamilyUsers
            .AnyAsync(fu => fu.FamilyId == request.FamilyId && fu.UserId == invitee.Id,
                cancellationToken);

        if (isAlreadyMember)
            return Result.Failure<InvitationResponse>(
                DomainError.InvalidState(nameof(Invitation), "User is already a member of this family."));

        // 4. Check if there's already a pending invitation
        bool hasPendingInvitation = await db.Invitations
            .AnyAsync(i =>
                i.FamilyId == request.FamilyId &&
                i.InviteeUserId == invitee.Id &&
                i.Status == InvitationStatus.Pending,
                cancellationToken);

        if (hasPendingInvitation)
            return Result.Failure<InvitationResponse>(
                DomainError.InvalidState(nameof(Invitation), "A pending invitation already exists for this user."));

        // 5. Create invitation (with event)
        Result<Invitation> invitationResult = Invitation.Create(
            invitee.Id,
            request.InviterUserId,
            request.FamilyId,
            request.IsParent,
            fireEvent: true);

        if (invitationResult.IsFailure)
            return Result.Failure<InvitationResponse>(invitationResult.TryGetError());

        Invitation invitation = invitationResult.TryGetValue();

        // 6. Save invitation
        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(cancellationToken);

        // 7. Return response
        InvitationResponse response = invitation.Adapt<InvitationResponse>();
        return Result.Success(response);
    }
}