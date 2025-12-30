using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Helpers;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Idenitity;

public class IdentityService(
    UserManager<ApplicationUser> userManager

) : IIdentityService
{



    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null && await userManager.IsInRoleAsync(user, role);
    }





    public async Task<Result<AuthenticatedUser>> AuthenticateByEmailAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<AuthenticatedUser>(
                IdentityUserError.NotFound($"User with email {UtilityService.MaskEmail(email)} not found"));

        if (!user.EmailConfirmed)
            return Result.Failure<AuthenticatedUser>(
                IdentityUserError.EmailNotConfirmed($"Email '{UtilityService.MaskEmail(email)}' is not confirmed"));



        if (!await userManager.CheckPasswordAsync(user, password))
            return Result.Failure<AuthenticatedUser>(IdentityUserError.InvalidCredentials());

        string? role = await GetRole(user);

        return Result.Success((user, role).Adapt<AuthenticatedUser>());
    }





    public async Task<Result<AuthenticatedUser>> GetUserByIdAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure<AuthenticatedUser>(IdentityUserError.NotFound());




        string? role = await GetRole(user);

        return Result.Success((user, role).Adapt<AuthenticatedUser>());
    }


    public async Task<Result<string>> GetFullNameAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Failure<string>(IdentityUserError.NotFound());

        return Result.Success<string>(user.UserName!);
    }

    public async Task<Result<IdentityRegistrationResult>> CreateIdentityByEmailAsync(
        string email,
        string password,
        string userName,

        CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return Result.Failure<IdentityRegistrationResult>(IdentityUserError.DuplicateEmail());

        Result<ApplicationUser> identityUserResult = ApplicationUser.Create(email, userName);

        if (identityUserResult.IsFailure)
            return Result.Failure<IdentityRegistrationResult>(identityUserResult.TryGetError());

        ApplicationUser identityUser = identityUserResult.TryGetValue();

        // Create with password
        var createIdentity = await userManager.CreateAsync(identityUser, password);

        if (!createIdentity.Succeeded)
        {
            string errors = string.Join(" | ", createIdentity.Errors.Select(e => e.Description));
            return Result.Failure<IdentityRegistrationResult>(IdentityUserError.CreationFailed(errors));
        }

        IdentityRegistrationResult registrationResult = new IdentityRegistrationResult(identityUser.Id.ToString());

        return Result.Success(registrationResult);
    }




    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Result.Failure(IdentityUserError.NotFound());


        if (currentPassword == newPassword)
        {
            return Result.Failure(
                IdentityUserError.PasswordChangeFailed("New password cannot be the same as the current password.")
            );
        }


        var checkPassword = await userManager.CheckPasswordAsync(user, currentPassword);
        if (!checkPassword)
        {
            return Result.Failure(
                IdentityUserError.PasswordMismatch("Current password is incorrect")
            );
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
            return Result.Success();

        if (result.Errors.Any(e =>
                e.Code.Contains("PasswordTooShort") ||
                e.Code.Contains("PasswordRequires")))
        {
            return Result.Failure(IdentityUserError.WeakPassword(ValidationMessages.WeakPassword));
        }

        return Result.Failure(
            IdentityUserError.PasswordChangeFailed("Failed to change password")
        );
    }

    public async Task<Result<AuthenticatedUser>> FindUserByEmailAsync(string email, bool requireConfirmedEmail = true)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);



        if (user is null)
            return Result.Failure<AuthenticatedUser>(IdentityUserError.NotFound("User not found"));

        if (requireConfirmedEmail)
        {
            if (!user.EmailConfirmed)
                return Result.Failure<AuthenticatedUser>(
                    IdentityUserError.EmailNotConfirmed($"Email '{UtilityService.MaskEmail(email)}' is not confirmed"));


        }


        string? role = await GetRole(user);

        return Result.Success((user, role).Adapt<AuthenticatedUser>());

    }



    public async Task<Result> IsUserConfirmedAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure(IdentityUserError.NotFound());

        if (user.EmailConfirmed || user.PhoneNumberConfirmed)
            return Result.Success();
        return Result.Failure(IdentityUserError.UnverifiedAccount());
    }
    public async Task<Result> IsUserNotConfirmedAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure(IdentityUserError.NotFound());

        if (user.EmailConfirmed || user.PhoneNumberConfirmed)
            return Result.Failure(IdentityUserError.DuplicatedConfirmation());
        return Result.Success();
    }

    private async Task<string?> GetRole(ApplicationUser user)
    {
        string? role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
        return role;
    }

    public async Task<string?> GetRoleByIdentityId(Guid Id)
    {
        var user = await userManager.FindByIdAsync(Id.ToString());
        if (user is null)
            return null;

        string? role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
        return role;
    }






    public async Task<Result<Guid>> ConfirmUserAsync(string email, CancellationToken ct)
    {
        var identityUser =
        await userManager.FindByEmailAsync(email)
        ?? await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == email, ct);

        if (identityUser is null)
            return Result.Failure<Guid>(IdentityUserError.NotFound());

        if (identityUser.Email != null && identityUser.EmailConfirmed)
            return Result.Failure<Guid>(IdentityUserError.DuplicatedConfirmation("Email already confirmed"));

        if (identityUser.Email != null)
            identityUser.EmailConfirmed = true;

        var updateResult = await userManager.UpdateAsync(identityUser);

        if (!updateResult.Succeeded)
        {
            string errors = string.Join(" | ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure<Guid>(IdentityUserError.UpdateFailed(errors));
        }


        return Result.Success<Guid>(identityUser.Id);
    }



    public async Task<Result> ResetPasswordAsync(
     Guid userId,
     string newPassword,
     CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Result.Failure(IdentityUserError.NotFound());

        if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
        {
            return Result.Failure(IdentityUserError.UnverifiedAccount(
                "User must verify email or phone before resetting password"));
        }

        bool samePassword = await userManager.CheckPasswordAsync(user, newPassword);
        if (samePassword)
        {
            return Result.Failure(IdentityUserError.SamePassword(
                "New password cannot be the same as the current password"));
        }

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            return Result.Failure(IdentityUserError.PasswordResetFailed(
                "Failed to clear existing password"));
        }

        var addResult = await userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded)
        {
            if (addResult.Errors.Any(e =>
                e.Code.Contains("PasswordTooShort") ||
                e.Code.Contains("PasswordRequires")))
            {
                return Result.Failure(IdentityUserError.WeakPassword(
                    "New password does not meet security requirements"));
            }

            return Result.Failure(IdentityUserError.PasswordResetFailed(
                "Failed to set new password"));
        }

        return Result.Success();
    }


}