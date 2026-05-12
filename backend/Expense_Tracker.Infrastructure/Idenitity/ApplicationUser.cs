using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.Identity;
using Expense_Tracker.Domain.Errors;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;

namespace Expense_Tracker.Infrastructure.Idenitity;

public sealed class ApplicationUser : IdentityUser<Guid>, ISoftDeletable
{
    public List<RefreshToken> RefreshTokens { get; private set; } = new();
    public bool IsDeleted { get; private set; }
    public Guid? DeletedById { get; private set; }
    public DateTimeOffset? DeletedOn { get; private set; }

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

    private ApplicationUser() { }

    public static ErrorOr<ApplicationUser> Create(string Email, string UserName)
    {
        if (string.IsNullOrWhiteSpace(Email))
            return DomainErrors.IdentityErrors.EmptyEmail();

        if (string.IsNullOrWhiteSpace(UserName))
            return DomainErrors.IdentityErrors.EmptyFullName();

        try
        {
            _ = new MailAddress(Email);
        }
        catch
        {
            return DomainErrors.IdentityErrors.InvalidEmail();
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = Email,
            NormalizedEmail = Email.ToUpperInvariant(),
            UserName = UserName,
            NormalizedUserName = UserName.ToUpperInvariant(),
        };

        return user;
    }

    public ErrorOr<Success> SoftDelete(Guid deletedBy)
    {
        if (IsDeleted)
            return DomainErrors.IdentityErrors.InvalidPermissions("User is already deleted.");

        IsDeleted = true;
        DeletedById = deletedBy;
        DeletedOn = DateTimeOffset.UtcNow;

        return new Success();
    }

    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted)
            return DomainErrors.IdentityErrors.InvalidPermissions("User is not deleted.");

        IsDeleted = false;
        DeletedById = null;
        DeletedOn = null;

        return new Success();
    }
}
