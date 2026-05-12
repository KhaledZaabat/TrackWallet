using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.Invitation.Enums;

namespace Expense_Tracker.Domain.Invitation;

public sealed class Invitation : Entity
{
    public Guid InviteeUserId { get; private set; }
    public Guid InviterUserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public bool IsParent { get; private set; }
    public DateTimeOffset SentAtUtc { get; private set; }
    public InvitationStatus Status { get; private set; }

    public Family Family { get; private set; } = null!;

    private Invitation() { }

    private Invitation(
        Guid id,
        Guid inviteeUserId,
        Guid inviterUserId,
        Guid familyId,
        bool isParent) : base(id)
    {
        InviteeUserId = inviteeUserId;
        InviterUserId = inviterUserId;
        FamilyId = familyId;
        IsParent = isParent;
        SentAtUtc = DateTimeOffset.UtcNow;
        Status = InvitationStatus.Pending;
    }

    public static ErrorOr<Invitation> Create(
        Guid inviteeUserId,
        Guid inviterUserId,
        Guid familyId,
        bool isParent)
    {
        if (inviteeUserId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Invitation), "Invitee user ID is required.");

        if (inviterUserId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Invitation), "Inviter user ID is required.");

        if (familyId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Invitation), "Family ID is required.");

        if (inviteeUserId == inviterUserId)
            return DomainErrors.InvitationErrors.SelfInvite();

        var invitation = new Invitation(
            Guid.CreateVersion7(),
            inviteeUserId,
            inviterUserId,
            familyId,
            isParent);

        return invitation;
    }

    public ErrorOr<Success> Accept()
    {
        if (Status == InvitationStatus.Accepted)
            return DomainErrors.InvitationErrors.AlreadyAccepted();

        if (Status == InvitationStatus.Declined)
            return DomainErrors.InvitationErrors.AlreadyDeclined();

        if (Status == InvitationStatus.Cancelled)
            return DomainErrors.InvitationErrors.Cancelled();

        Status = InvitationStatus.Accepted;
        return new Success();
    }

    public ErrorOr<Success> Decline()
    {
        if (Status == InvitationStatus.Declined)
            return DomainErrors.InvitationErrors.AlreadyDeclined();

        if (Status == InvitationStatus.Accepted)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Invitation), "Invitation was accepted and cannot be declined.");

        if (Status == InvitationStatus.Cancelled)
            return DomainErrors.InvitationErrors.Cancelled();

        Status = InvitationStatus.Declined;
        return new Success();
    }

    public ErrorOr<Success> Cancel(Guid requesterId)
    {
        if (Status != InvitationStatus.Pending)
            return DomainErrors.InvitationErrors.NotPending();

        Status = InvitationStatus.Cancelled;
        return new Success();
    }
}
