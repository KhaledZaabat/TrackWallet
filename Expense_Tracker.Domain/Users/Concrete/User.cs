using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Domain.Users.Abstraction;
using System.Net.Mail;

namespace Expense_Tracker.Domain.Users;

public sealed class User : AggregateRoot, IAuditable, ISoftDeletable
{
    public Guid? ProfileImageFileId { get; private set; }
    public UploadedFile? ProfileImage { get; private set; }

    public string FullName { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public DateOnly BirthDate { get; private set; }
    public bool IsMale { get; private set; }

    public Role Role { get; private set; } = Role.Child;

    // Audit properties
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; } = Guid.Empty;
    public DateTimeOffset LastModifiedUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid LastModifiedBy { get; private set; } = Guid.Empty;

    // Soft delete properties
    public bool IsDeleted { get; private set; }
    public Guid? DeletedById { get; private set; } = Guid.Empty;
    public DateTimeOffset? DeletedOn { get; private set; }

    // Explicit interface implementations for IAuditable
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

    // Explicit interface implementations for ISoftDeletable
    bool ISoftDeletable.IsDeleted
    {
        get => IsDeleted;
        set => IsDeleted = value;
    }

    Guid? ISoftDeletable.DeletedById
    {
        get => DeletedById;
        set => DeletedById = value;
    }

    DateTimeOffset? ISoftDeletable.DeletedOn
    {
        get => DeletedOn;
        set => DeletedOn = value;
    }

    // EF Core constructor
    private User() { }

    private User(
        Guid id,
        string fullName,
        string userName,
        string email,
        DateOnly birthDate,
        bool isMale) : base(id)
    {
        FullName = fullName;
        UserName = userName;
        Email = email;
        BirthDate = birthDate;
        IsMale = isMale;
    }

    /// <summary>
    /// Creates a domain user. Id must be the same Guid as the Identity ApplicationUser.Id.
    /// </summary>
    public static Result<User> Create(
        Guid id,
        string fullName,
        string userName,
        string email,
        DateOnly birthDate,
        bool isMale,
        bool fireEvent = true)
    {
        if (id == Guid.Empty)
            return Result.Failure<User>(UserError.InvalidSubmission("User Id is required."));

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<User>(UserError.InvalidSubmission("Full name is required."));

        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure<User>(UserError.InvalidSubmission("Username is required."));

        if (userName.Length > 50)
            return Result.Failure<User>(UserError.InvalidSubmission("Username cannot exceed 50 characters."));

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(UserError.InvalidSubmission("Email is required."));

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return Result.Failure<User>(UserError.InvalidSubmission("Invalid email format."));
        }

        if (birthDate >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure<User>(UserError.InvalidSubmission("Birth date must be in the past."));

        var user = new User(
            id,
            fullName.Trim(),
            userName.Trim(),
            email.Trim().ToLowerInvariant(),
            birthDate,
            isMale

        );

        if (fireEvent)
            user.AddDomainEvent(new UserCreatedEvent(user));

        return Result.Success(user);
    }

    public void FireUserCreatedEvent()
    {
        this.AddDomainEvent(new UserCreatedEvent(this));
    }

    public Result AssignProfileImage(Guid fileId)
    {
        if (fileId == Guid.Empty)
            return Result.Failure(
                UserError.InvalidSubmission("Profile image id cannot be empty."));

        ProfileImageFileId = fileId;
        return Result.Success();
    }

    public Result UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(UserError.InvalidSubmission("Full name cannot be empty."));

        if (fullName.Length > 100)
            return Result.Failure(UserError.InvalidSubmission("Full name cannot exceed 100 characters."));

        FullName = fullName.Trim();
        return Result.Success();
    }

    public Result UpdateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure(UserError.InvalidSubmission("Username cannot be empty."));

        if (userName.Length > 50)
            return Result.Failure(UserError.InvalidSubmission("Username cannot exceed 50 characters."));

        UserName = userName.Trim();
        return Result.Success();
    }

    public Result UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(UserError.InvalidSubmission("Email cannot be empty."));

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return Result.Failure(UserError.InvalidSubmission("Invalid email format."));
        }

        Email = email.Trim().ToLowerInvariant();
        return Result.Success();
    }

    public Result UpdateBirthDate(DateOnly birthDate)
    {
        if (birthDate >= DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(UserError.InvalidSubmission("Birth date must be in the past."));

        BirthDate = birthDate;
        return Result.Success();
    }

    public Result UpdateGender(bool isMale)
    {
        IsMale = isMale;
        return Result.Success();
    }

    public Result UpdateRole(Role role)
    {
        Role = role;
        return Result.Success();
    }

    public Result SoftDelete(Guid deletedBy)
    {
        if (IsDeleted)
        {
            return Result.Failure(
                DomainError.InvalidState(nameof(User), "User is already deleted."));
        }

        IsDeleted = true;
        DeletedById = deletedBy;
        DeletedOn = DateTimeOffset.UtcNow;

        // Clear sensitive data
        Email = $"deleted_{Id}@deleted.local";
        return Result.Success();
    }
    public Result UpgradeToParent()
    {
        Role = Role.Parent;
        return Result.Success();

    }

}