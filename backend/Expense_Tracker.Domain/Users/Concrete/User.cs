using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using System.Net.Mail;

namespace Expense_Tracker.Domain.Users;

public sealed class User : Entity, IAuditable, ISoftDeletable
{
    public Guid? ProfileImageFileId { get; private set; }
    public UploadedFile? ProfileImage { get; private set; }

    public string FullName { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;


    public string NormalizedUserName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public DateOnly? BirthDate { get; private set; }
    public bool? IsMale { get; private set; }

    public Guid NotificationPreferencesId { get; private set; } = NotificationPreferences.DefaultNotificationId;
    public NotificationPreferences NotificationPreferences { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; private set; } = Guid.Empty;
    public DateTimeOffset LastModifiedUtc { get; private set; } = DateTimeOffset.UtcNow;
    public Guid LastModifiedBy { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; }
    public Guid? DeletedById { get; private set; } = Guid.Empty;
    public DateTimeOffset? DeletedOn { get; private set; }

    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

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

    private User() { }

    private User(
        Guid id,
        string fullName,
        string userName,
        string email,
        DateOnly? birthDate,
        bool? isMale) : base(id)
    {
        FullName = fullName;
        UserName = userName;
        NormalizedUserName = Normalize(userName);
        Email = email;
        BirthDate = birthDate;
        IsMale = isMale;
    }


    public static string Normalize(string userName) =>
        userName?.Trim().ToUpperInvariant() ?? string.Empty;

    public static ErrorOr<User> Create(
        Guid id,
        string fullName,
        string userName,
        string email,
        DateOnly? birthDate = null,
        bool? isMale = null)
    {
        if (id == Guid.Empty)
            return DomainErrors.UserErrors.InvalidSubmission("User Id is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return DomainErrors.UserErrors.InvalidSubmission("Full name is required.");

        if (string.IsNullOrWhiteSpace(userName))
            return DomainErrors.UserErrors.InvalidSubmission("Username is required.");

        if (userName.Length > 50)
            return DomainErrors.UserErrors.InvalidSubmission("Username cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(email))
            return DomainErrors.UserErrors.InvalidSubmission("Email is required.");

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return DomainErrors.UserErrors.InvalidSubmission("Invalid email format.");
        }

        if (birthDate is not null && birthDate >= DateOnly.FromDateTime(DateTime.Today))
            return DomainErrors.UserErrors.InvalidSubmission("Birth date must be in the past.");

        var user = new User(
            id,
            fullName.Trim(),
            userName.Trim(),
            email.Trim().ToLowerInvariant(),
            birthDate,
            isMale
        );

        return user;
    }

    public ErrorOr<Success> AssignProfileImage(Guid fileId)
    {
        if (fileId == Guid.Empty)
            return DomainErrors.UserErrors.InvalidSubmission("Profile image id cannot be empty.");

        ProfileImageFileId = fileId;
        return new Success();
    }

    public ErrorOr<Success> UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return DomainErrors.UserErrors.InvalidSubmission("Full name cannot be empty.");

        if (fullName.Length > 100)
            return DomainErrors.UserErrors.InvalidSubmission("Full name cannot exceed 100 characters.");

        FullName = fullName.Trim();
        return new Success();
    }

    public ErrorOr<Success> UpdateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return DomainErrors.UserErrors.InvalidSubmission("Username cannot be empty.");

        if (userName.Length > 50)
            return DomainErrors.UserErrors.InvalidSubmission("Username cannot exceed 50 characters.");

        UserName = userName.Trim();
        NormalizedUserName = Normalize(userName);
        return new Success();
    }

    public ErrorOr<Success> UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return DomainErrors.UserErrors.InvalidSubmission("Email cannot be empty.");

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return DomainErrors.UserErrors.InvalidSubmission("Invalid email format.");
        }

        Email = email.Trim().ToLowerInvariant();
        return new Success();
    }

    public ErrorOr<Success> UpdateBirthDate(DateOnly birthDate)
    {
        if (birthDate >= DateOnly.FromDateTime(DateTime.Today))
            return DomainErrors.UserErrors.InvalidSubmission("Birth date must be in the past.");

        BirthDate = birthDate;
        return new Success();
    }

    public ErrorOr<Success> UpdateGender(bool isMale)
    {
        IsMale = isMale;
        return new Success();
    }

    public ErrorOr<Success> SoftDelete(Guid deletedBy)
    {
        if (IsDeleted)
            return DomainErrors.GeneralErrors.InvalidState(nameof(User), "User is already deleted.");

        IsDeleted = true;
        DeletedById = deletedBy;
        DeletedOn = DateTimeOffset.UtcNow;

        Email = $"deleted_{Id}@deleted.local";
        return new Success();
    }

    public ErrorOr<Success> UpdateNotificationPreferences(Guid npId)
    {
        NotificationPreferencesId = npId;
        return new Success();
    }
}
