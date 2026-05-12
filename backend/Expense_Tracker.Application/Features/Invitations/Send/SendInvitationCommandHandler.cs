using Expense_Tracker.Application.Interfaces;
using ErrorOr;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.Invitation.Enums;
using Expense_Tracker.Domain.Users;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Invitations.Send;

public sealed class SendInvitationCommandHandler(
    IRepository<FamilyUser> familyUserRepo,
    IRepository<User> userRepo,
    IRepository<Invitation> invitationRepo,
    IMessageBus bus)
{
    public async Task<ErrorOr<InvitationResponse>> Handle(
        SendInvitationCommand request,
        CancellationToken cancellationToken)
    {
        bool isInviterParent = await familyUserRepo.QueryTracked()
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.InviterUserId &&
                fu.IsParent,
                cancellationToken);

        if (!isInviterParent)
            return DomainErrors.GeneralErrors.Forbidden("Only parent members can send invitations.");

        User? invitee = await userRepo.QueryTracked()
            .FirstOrDefaultAsync(u => u.Email == request.InviteeEmail.Trim().ToLowerInvariant(),
                cancellationToken);

        if (invitee is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(User));

        bool isAlreadyMember = await familyUserRepo.QueryTracked()
            .AnyAsync(fu => fu.FamilyId == request.FamilyId && fu.UserId == invitee.Id,
                cancellationToken);

        if (isAlreadyMember)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Invitation), "User is already a member of this family.");

        bool hasPendingInvitation = await invitationRepo.QueryTracked()
            .AnyAsync(i =>
                i.FamilyId == request.FamilyId &&
                i.InviteeUserId == invitee.Id &&
                i.Status == InvitationStatus.Pending,
                cancellationToken);

        if (hasPendingInvitation)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Invitation), "A pending invitation already exists for this user.");

        ErrorOr<Invitation> invitationResult = Invitation.Create(
            invitee.Id,
            request.InviterUserId,
            request.FamilyId,
            request.IsParent);

        if (invitationResult.IsError)
            return invitationResult.Errors;

        Invitation invitation = invitationResult.Value;

        await invitationRepo.AddAsync(invitation);
        await invitationRepo.SaveChangesAsync(cancellationToken);

        await bus.PublishAsync(new InvitationCreatedEvent(invitation));

        InvitationResponse response = invitation.Adapt<InvitationResponse>();
        return response;
    }
}
