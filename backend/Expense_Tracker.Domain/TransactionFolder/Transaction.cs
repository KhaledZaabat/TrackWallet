using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using Expense_Tracker.Domain.Users;

namespace Expense_Tracker.Domain.TransactionFolder;

public sealed class Transaction : AggregateRoot, IAuditable
{
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly TransactedOn { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;

    // Foreign Keys
    public Guid CreatedById { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid CategoryId { get; private set; }

    // Navigation Properties
    public User? CreatedBy { get; private set; }
    public Family? Family { get; private set; }
    public Category? Category { get; private set; }

    // Audit properties
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy_AuditId { get; private set; } = Guid.Empty;
    public DateTimeOffset LastModifiedUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid LastModifiedBy { get; private set; } = Guid.Empty;

    // Explicit interface implementations for IAuditable
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

    // EF Core constructor
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

    public static Result<Transaction> Create(
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
            return Result.Failure<Transaction>(
                DomainError.InvalidState(nameof(Transaction), "TransactionFolder title cannot be empty."));

        if (amount <= 0)
            return Result.Failure<Transaction>(
                DomainError.InvalidState(nameof(Transaction), "Amount must be greater than zero."));

        if (createdByID == Guid.Empty)
            return Result.Failure<Transaction>(
                DomainError.InvalidState(nameof(Transaction), "CreatedBy ID cannot be empty."));

        if (familyID == Guid.Empty)
            return Result.Failure<Transaction>(
                DomainError.InvalidState(nameof(Transaction), "Family ID cannot be empty."));

        if (categoryID == Guid.Empty)
            return Result.Failure<Transaction>(
                DomainError.InvalidState(nameof(Transaction), "Category ID cannot be empty."));

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
        transaction.AddDomainEvent(new TransactionCreatedEvent(transaction));

        return Result.Success(transaction);
    }

    public Result Rename(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            return Result.Failure(
                DomainError.InvalidState(nameof(Transaction), "TransactionFolder title cannot be empty."));

        Title = newTitle.Trim();
        return Result.Success();
    }

    public Result ChangeNotes(string newNotes)
    {
        Notes = newNotes ?? string.Empty;
        return Result.Success();
    }

    public Result ChangeAmount(decimal newAmount)
    {
        if (newAmount <= 0)
            return Result.Failure(
                DomainError.InvalidState(nameof(Transaction), "Amount must be greater than zero."));

        Amount = newAmount;
        return Result.Success();
    }

    public Result MoveToCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
            return Result.Failure(
                DomainError.InvalidState(nameof(Transaction), "Category ID cannot be empty."));

        CategoryId = newCategoryId;
        return Result.Success();
    }

    public Result Reschedule(DateOnly newDate)
    {
        TransactedOn = newDate;
        return Result.Success();
    }

    public Result MarkAsIncome()
    {
        Type = TransactionType.Income;
        return Result.Success();
    }

    public Result MarkAsExpense()
    {
        Type = TransactionType.Expense;
        return Result.Success();
    }
}

