using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;


namespace Expense_Tracker.Domain.Users.Abstraction;




public class User : AggregateRoot, IAuditable, ISoftDeletable
{
    public string? PictureUrl { get; private set; } = string.Empty;

    public string FullName { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;

    public Role Role { get; private set; }


    public Guid NotificationPreferencesId { get; protected set; } = NotificationPreferences.DefaultNotificationId;
    public NotificationPreferences? Notifications { get; protected set; }

    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; protected set; } = Guid.Empty;
    public DateTimeOffset LastModifiedUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public Guid LastModifiedBy { get; protected set; } = Guid.Empty;


    public bool IsDeleted { get; protected set; }
    public Guid? DeletedById { get; protected set; } = Guid.Empty;
    public DateTimeOffset? DeletedOn { get; protected set; }



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


    protected User() { }

    protected User(Guid id, string fullName, string email, Role role) : base(id)
    {
        FullName = fullName;
        Email = email;
        Role = role;
    }


    public Result UpdateNotifications(NotificationPreferences notifications)
    {
        if (notifications is null)
            return Result.Failure(
                DomainError.InvalidState(nameof(NotificationPreferences), "Notifications cannot be null."));

        var validation = NotificationPreferences.Create(
            notifications.EmailNotifications,
            notifications.PushNotifications

        );

        if (validation.IsFailure)
            return Result.Failure(validation.TryGetError());

        var newPref = validation.TryGetValue();

        Notifications = newPref;
        NotificationPreferencesId = newPref.Id;

        return Result.Success();
    }
    public void FireUserCreatedEvent(Guid id, string email, string fullName, string role)
    {
        this.AddDomainEvent(new ResetPasswordEvent(id, email, fullName, role));
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

        return Result.Success();
    }




}
