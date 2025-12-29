using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
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



    private ApplicationUser() { } // EF Core

    public static Result<ApplicationUser> Create(string email, string UserName)
    {
        // Email null or empty
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<ApplicationUser>(
                IdentityUserError.Validation("Email cannot be empty.")
            );
        }

        // Full name null or empty
        if (string.IsNullOrWhiteSpace(UserName))
        {
            return Result.Failure<ApplicationUser>(
                IdentityUserError.Validation("Full name cannot be empty.")
            );
        }

        // Email format validation
        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return Result.Failure<ApplicationUser>(
                IdentityUserError.Validation("Email format is invalid.")
            );
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),


            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),

            UserName = UserName,
            NormalizedUserName = UserName.ToUpperInvariant(),

            EmailConfirmed = true
        };


        return Result.Success(user);
    }

    public Result SoftDelete(Guid deletedBy)
    {
        if (IsDeleted)
            return Result.Failure(
                IdentityUserError.Validation("User is already deleted."));

        IsDeleted = true;
        DeletedById = deletedBy;
        DeletedOn = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result Restore()
    {
        if (!IsDeleted)
            return Result.Failure(
                IdentityUserError.Validation("User is not deleted."));

        IsDeleted = false;
        DeletedById = null;
        DeletedOn = null;

        return Result.Success();
    }
}