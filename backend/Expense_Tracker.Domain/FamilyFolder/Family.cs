using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.TransactionFolder;

namespace Expense_Tracker.Domain.FamilyFolder;

public sealed class Family : Entity, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public decimal CurrentBudget { get; private set; }

    public string? FamilyBio { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; } = Guid.Empty;
    public DateTimeOffset LastModifiedUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid LastModifiedBy { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; }
    public Guid? DeletedById { get; private set; }
    public DateTimeOffset? DeletedOn { get; private set; }
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<FamilyUser> FamilyUsers { get; private set; } = new List<FamilyUser>();

    DateTimeOffset ICreatable.CreatedAtUtc
    {
        get => CreatedAtUtc;
        set => CreatedAtUtc = value;
    }

    Guid ICreatable.CreatedBy
    {
        get => CreatedBy;
        set => CreatedBy = value;
    }

    DateTimeOffset IUpdatable.LastModifiedUtc
    {
        get => LastModifiedUtc;
        set => LastModifiedUtc = value;
    }

    Guid IUpdatable.LastModifiedBy
    {
        get => LastModifiedBy;
        set => LastModifiedBy = value;
    }

    private Family() { }

    private Family(Guid id, string name, decimal currentBudget, string? familyBio) : base(id)
    {
        Name = name;
        CurrentBudget = currentBudget;
        FamilyBio = familyBio;
    }

    public static ErrorOr<Family> Create(string name, decimal currentBudget, Guid createdBy, string? familyBio = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Family name is required.");

        if (name.Length > 100)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Family name cannot exceed 100 characters.");

        if (currentBudget < 0)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Current budget cannot be negative.");

        var family = new Family(Guid.CreateVersion7(), name.Trim(), currentBudget, familyBio);
        family.CreatedBy = createdBy;

        return family;
    }

    public ErrorOr<Success> UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Family name cannot be empty.");

        if (name.Length > 100)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Family name cannot exceed 100 characters.");

        Name = name.Trim();
        return new Success();
    }

    public ErrorOr<Success> ApplyTransaction(decimal amount, bool isExpense)
    {
        if (amount <= 0)
            return DomainErrors.TransactionErrors.InvalidAmount(amount);

        if (isExpense && amount > CurrentBudget)
            return DomainErrors.TransactionErrors.BudgetNotEnough(CurrentBudget, amount);

        if (isExpense)
            CurrentBudget -= amount;
        else
            CurrentBudget += amount;

        return new Success();
    }

    public ErrorOr<Success> ReverseTransaction(decimal amount, bool isExpense)
    {
        if (amount <= 0)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Transaction amount must be greater than zero.");

        if (isExpense)
            CurrentBudget += amount;
        else
            CurrentBudget -= amount;

        return new Success();
    }

    public ErrorOr<Success> UpdateBudget(decimal newBudget)
    {
        if (newBudget < 0)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Budget cannot be negative.");

        CurrentBudget = newBudget;
        return new Success();
    }

    public ErrorOr<Success> SoftDelete(Guid deletedBy)
    {
        if (IsDeleted)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "Family is already deleted.");

        IsDeleted = true;
        DeletedById = deletedBy;
        DeletedOn = DateTimeOffset.UtcNow;

        return new Success();
    }

    public ErrorOr<Success> UpdateBio(string? bio)
    {
        FamilyBio = bio;
        return new Success();
    }
}
