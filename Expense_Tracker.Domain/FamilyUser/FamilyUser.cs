using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyFolder;

namespace Expense_Tracker.Domain.FamilyUser;


public sealed class FamilyUser : Entity
{
    public Guid FamilyId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsParent { get; private set; }
    public Guid InvitedById { get; private set; }
    public DateTimeOffset JoinedAtUtc { get; private set; }

    // Navigation properties
    public Family Family { get; private set; } = null!;
    public Users.User User { get; private set; } = null!;

    // EF Core constructor
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

    public static Result<FamilyUser> Create(
        Guid familyId,
        Guid userId,
        bool isParent,
        Guid invitedById)
    {
        if (familyId == Guid.Empty)
            return Result.Failure<FamilyUser>(
                DomainError.InvalidState(nameof(FamilyUser), "Family ID is required."));

        if (userId == Guid.Empty)
            return Result.Failure<FamilyUser>(
                DomainError.InvalidState(nameof(FamilyUser), "User ID is required."));

        if (invitedById == Guid.Empty)
            return Result.Failure<FamilyUser>(
                DomainError.InvalidState(nameof(FamilyUser), "Inviter ID is required."));

        var familyUser = new FamilyUser(
            Guid.CreateVersion7(),
            familyId,
            userId,
            isParent,
            invitedById);

        return Result.Success(familyUser);
    }

    public Result PromoteToParent()
    {
        if (IsParent)
            return Result.Failure(
                DomainError.InvalidState(nameof(FamilyUser), "User is already a parent."));

        IsParent = true;
        return Result.Success();
    }

    public Result DemoteToChild()
    {
        if (!IsParent)
            return Result.Failure(
                DomainError.InvalidState(nameof(FamilyUser), "User is already a child."));

        IsParent = false;
        return Result.Success();
    }
}
