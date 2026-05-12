using ErrorOr;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using Expense_Tracker.Domain.Users;

namespace Expense_Tracker.Domain.TransactionFolder;

public sealed class Transaction : Entity, IAuditable
{
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly TransactedOn { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;

    public Guid CreatedById { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid CategoryId { get; private set; }

    public User? CreatedBy { get; private set; }
    public Family? Family { get; private set; }
    public Category? Category { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy_AuditId { get; private set; } = Guid.Empty;
    public DateTimeOffset LastModifiedUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid LastModifiedBy { get; private set; } = Guid.Empty;

    DateTimeOffset ICreatable.CreatedAtUtc
    {
        get => CreatedAtUtc;
        set => CreatedAtUtc = value;
    }

    Guid ICreatable.CreatedBy
    {
        get => CreatedBy_AuditId;
        set => CreatedBy_AuditId = value;
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

    private Transaction() { }

    private Transaction(
        Guid id,
        TransactionType type,
        decimal amount,
        DateOnly transactedOn,
        string title,
        string notes,
        Guid createdByID,
        Guid familyID,
        Guid categoryID) : base(id)
    {
        Type = type;
        Amount = amount;
        TransactedOn = transactedOn;
        Title = title;
        Notes = notes;
        CreatedById = createdByID;
        FamilyId = familyID;
        CategoryId = categoryID;
        CreatedBy_AuditId = createdByID;
    }

    public static ErrorOr<Transaction> Create(
        TransactionType type,
        decimal amount,
        DateOnly transactedOn,
        string title,
        string notes,
        Guid createdByID,
        Guid familyID,
        Guid categoryID)
    {
        if (string.IsNullOrWhiteSpace(title))
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Transaction title cannot be empty.");

        if (amount <= 0)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Amount must be greater than zero.");

        if (createdByID == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "CreatedBy ID cannot be empty.");

        if (familyID == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Family ID cannot be empty.");

        if (categoryID == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Category ID cannot be empty.");

        var transaction = new Transaction(
            Guid.CreateVersion7(),
            type,
            amount,
            transactedOn,
            title.Trim(),
            notes ?? string.Empty,
            createdByID,
            familyID,
            categoryID);

        return transaction;
    }

    public ErrorOr<Success> Rename(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Transaction title cannot be empty.");

        Title = newTitle.Trim();
        return new Success();
    }

    public ErrorOr<Success> ChangeNotes(string newNotes)
    {
        Notes = newNotes ?? string.Empty;
        return new Success();
    }

    public ErrorOr<Success> ChangeAmount(decimal newAmount)
    {
        if (newAmount <= 0)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Amount must be greater than zero.");

        Amount = newAmount;
        return new Success();
    }

    public ErrorOr<Success> MoveToCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Transaction), "Category ID cannot be empty.");

        CategoryId = newCategoryId;
        return new Success();
    }

    public ErrorOr<Success> Reschedule(DateOnly newDate)
    {
        TransactedOn = newDate;
        return new Success();
    }

    public ErrorOr<Success> MarkAsIncome()
    {
        Type = TransactionType.Income;
        return new Success();
    }

    public ErrorOr<Success> MarkAsExpense()
    {
        Type = TransactionType.Expense;
        return new Success();
    }
}
