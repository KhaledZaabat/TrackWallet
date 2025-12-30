using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyFolder;

namespace Expense_Tracker.Domain.FamilyUserFolder;

public sealed class FamilyBudgetHistory : Entity
{
    public Guid FamilyId { get; private set; }
    public decimal Budget { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    // Navigation property
    public Family Family { get; private set; } = null!;

    // EF Core constructor
    private FamilyBudgetHistory() { }

    private FamilyBudgetHistory(
        Guid id,
        Guid familyId,
        decimal budget,
        DateTimeOffset recordedAt) : base(id)
    {
        FamilyId = familyId;
        Budget = budget;
        RecordedAtUtc = recordedAt;
    }

    public static Result<FamilyBudgetHistory> Create(
        Guid familyId,
        decimal budget,
        DateTimeOffset? recordedAt = null)
    {
        if (familyId == Guid.Empty)
            return Result.Failure<FamilyBudgetHistory>(
                DomainError.InvalidState(nameof(FamilyBudgetHistory), "Family ID is required."));

        var history = new FamilyBudgetHistory(
            Guid.CreateVersion7(),
            familyId,
            budget,
            recordedAt ?? DateTimeOffset.UtcNow);

        return Result.Success(history);
    }
}
