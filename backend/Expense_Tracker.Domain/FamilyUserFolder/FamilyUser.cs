using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.FamilyFolder;

namespace Expense_Tracker.Domain.FamilyUserFolder;

public sealed class FamilyUser : Entity
{
    public Guid FamilyId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsParent { get; private set; }
    public Guid InvitedById { get; private set; }
    public DateTimeOffset JoinedAtUtc { get; private set; }

    public Family Family { get; private set; } = null!;
    public Users.User User { get; private set; } = null!;

    private FamilyUser() { }

    private FamilyUser(
        Guid id,
        Guid familyId,
        Guid userId,
        bool isParent,
        Guid invitedById) : base(id)
    {
        FamilyId = familyId;
        UserId = userId;
        IsParent = isParent;
        InvitedById = invitedById;
        JoinedAtUtc = DateTimeOffset.UtcNow;
    }

    public static ErrorOr<FamilyUser> Create(
        Guid familyId,
        Guid userId,
        bool isParent,
        Guid invitedById)
    {
        if (familyId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(FamilyUser), "Family ID is required.");

        if (userId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(FamilyUser), "User ID is required.");

        if (invitedById == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(FamilyUser), "Inviter ID is required.");

        var familyUser = new FamilyUser(
            Guid.CreateVersion7(),
            familyId,
            userId,
            isParent,
            invitedById);

        return familyUser;
    }

    public ErrorOr<Success> PromoteToParent()
    {
        if (IsParent)
            return DomainErrors.GeneralErrors.InvalidState(nameof(FamilyUser), "User is already a parent.");

        IsParent = true;
        return new Success();
    }

    public ErrorOr<Success> DemoteToChild()
    {
        if (!IsParent)
            return DomainErrors.GeneralErrors.InvalidState(nameof(FamilyUser), "User is already a child.");

        IsParent = false;
        return new Success();
    }
}
