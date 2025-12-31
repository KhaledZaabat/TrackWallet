using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.Invitation.Enums;

namespace Expense_Tracker.Domain.Invitation;

public sealed class Invitation : AggregateRoot
{
    public Guid InviteeUserId { get; private set; }
    public Guid InviterUserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public bool IsParent { get; private set; }
    public DateTimeOffset SentAtUtc { get; private set; }
    public InvitationStatus Status { get; private set; }

    // Navigation properties
    public Family Family { get; private set; } = null!;

    // EF Core constructor
    private Invitation() { }

    private Invitation(
        Guid id,
        Guid inviteeUserId,
        Guid inviterUserId,
        Guid familyId,
        bool isParent
       ) : base(id)
    {
        InviteeUserId = inviteeUserId;
        InviterUserId = inviterUserId;
        FamilyId = familyId;
        IsParent = isParent;
        SentAtUtc = DateTimeOffset.UtcNow;
        Status = InvitationStatus.Pending;


    }

    public static Result<Invitation> Create(
        Guid inviteeUserId,
        Guid inviterUserId,
        Guid familyId,
        bool isParent,
        bool fireEvent = true)
    {
        if (inviteeUserId == Guid.Empty)
            return Result.Failure<Invitation>(
                DomainError.InvalidState(nameof(Invitation), "Invitee user ID is required."));

        if (inviterUserId == Guid.Empty)
            return Result.Failure<Invitation>(
                DomainError.InvalidState(nameof(Invitation), "Inviter user ID is required."));

        if (familyId == Guid.Empty)
            return Result.Failure<Invitation>(
                DomainError.InvalidState(nameof(Invitation), "Family ID is required."));

        if (inviteeUserId == inviterUserId)
            return Result.Failure<Invitation>(
                DomainError.InvalidState(nameof(Invitation), "Cannot invite yourself."));

        var invitation = new Invitation(
            Guid.CreateVersion7(),
            inviteeUserId,
            inviterUserId,
            familyId,
            isParent
           );
        if (fireEvent)
            invitation.AddDomainEvent(
         new InvitationCreatedEvent(invitation));

        return Result.Success(invitation);
    }

    public Result Accept()
    {
        if (Status == InvitationStatus.Accepted)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Invitation already accepted."));

        if (Status == InvitationStatus.Declined)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Invitation was declined and cannot be accepted."));

        if (Status == InvitationStatus.Cancelled)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Invitation was cancelled and cannot be accepted."));



        Status = InvitationStatus.Accepted;
        AddDomainEvent(new InvitationAcceptedEvent(this));

        return Result.Success();
    }

    public Result Decline()
    {
        if (Status == InvitationStatus.Declined)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Invitation already declined."));

        if (Status == InvitationStatus.Accepted)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Invitation was accepted and cannot be declined."));

        if (Status == InvitationStatus.Cancelled)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Invitation was cancelled."));

        Status = InvitationStatus.Declined;
        AddDomainEvent(new InvitationDeclinedEvent(this));

        return Result.Success();
    }

    public Result Cancel(Guid requesterId)
    {
        if (requesterId != InviterUserId)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Only the inviter can cancel this invitation."));

        if (Status != InvitationStatus.Pending)
            return Result.Failure(
                DomainError.InvalidState(nameof(Invitation), "Only pending invitations can be cancelled."));

        Status = InvitationStatus.Cancelled;
        AddDomainEvent(new InvitationCancelledEvent(this));

        return Result.Success();
    }
}