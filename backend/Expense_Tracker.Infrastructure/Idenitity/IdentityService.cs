using ErrorOr;
using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Helpers;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Idenitity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user != null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task<ErrorOr<AuthenticatedUser>> AuthenticateByEmailAsync(
        string email,
        string password
    )
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return DomainErrors.IdentityErrors.NotFound(
                $"User with email {UtilityService.MaskEmail(email)} not found"
            );

        if (!user.EmailConfirmed)
            return DomainErrors.IdentityErrors.EmailNotConfirmed(
                $"Email '{UtilityService.MaskEmail(email)}' is not confirmed"
            );

        if (!await userManager.CheckPasswordAsync(user, password))
            return DomainErrors.IdentityErrors.InvalidCredentials();

        string? role = await GetRole(user);

        return (user, role).Adapt<AuthenticatedUser>();
    }

    public async Task<ErrorOr<AuthenticatedUser>> GetUserByIdAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        string? role = await GetRole(user);

        return (user, role).Adapt<AuthenticatedUser>();
    }

    public async Task<ErrorOr<string>> GetFullNameAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        return user.UserName!;
    }

    public async Task<ErrorOr<IdentityRegistrationResult>> CreateIdentityByEmailAsync(
        string email,
        string password,
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return DomainErrors.IdentityErrors.DuplicateEmail();

        var identityUserResult = ApplicationUser.Create(email, userName);

        if (identityUserResult.IsError)
            return identityUserResult.Errors;

        ApplicationUser identityUser = identityUserResult.Value;

        // Create with password
        var createIdentity = await userManager.CreateAsync(identityUser, password);

        if (!createIdentity.Succeeded)
        {
            string errors = string.Join(" | ", createIdentity.Errors.Select(e => e.Description));
            return DomainErrors.IdentityErrors.CreationFailed(errors);
        }

        IdentityRegistrationResult registrationResult = new IdentityRegistrationResult(
            identityUser.Id.ToString()
        );

        return registrationResult;
    }

    public async Task<ErrorOr<Success>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword
    )
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (currentPassword == newPassword)
        {
            return DomainErrors.IdentityErrors.PasswordChangeFailed(
                "New password cannot be the same as the current password."
            );
        }

        var checkPassword = await userManager.CheckPasswordAsync(user, currentPassword);
        if (!checkPassword)
        {
            return DomainErrors.IdentityErrors.PasswordMismatch("Current password is incorrect");
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
            return new Success();

        if (
            result.Errors.Any(e =>
                e.Code.Contains("PasswordTooShort") || e.Code.Contains("PasswordRequires")
            )
        )
        {
            return DomainErrors.IdentityErrors.WeakPassword(ValidationMessages.WeakPassword);
        }

        return DomainErrors.IdentityErrors.PasswordChangeFailed("Failed to change password");
    }

    public async Task<ErrorOr<AuthenticatedUser>> FindUserByEmailAsync(
        string email,
        bool requireConfirmedEmail = true
    )
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return DomainErrors.IdentityErrors.NotFound("User not found");

        if (requireConfirmedEmail)
        {
            if (!user.EmailConfirmed)
                return DomainErrors.IdentityErrors.EmailNotConfirmed(
                    $"Email '{UtilityService.MaskEmail(email)}' is not confirmed"
                );
        }

        string? role = await GetRole(user);

        return (user, role).Adapt<AuthenticatedUser>();
    }

    public async Task<ErrorOr<Success>> IsUserConfirmedAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (user.EmailConfirmed || user.PhoneNumberConfirmed)
            return new Success();
        return DomainErrors.IdentityErrors.UnverifiedAccount();
    }

    public async Task<ErrorOr<Success>> IsUserNotConfirmedAsync(string email, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (user.EmailConfirmed || user.PhoneNumberConfirmed)
            return DomainErrors.IdentityErrors.DuplicatedConfirmation();
        return new Success();
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

    public async Task<ErrorOr<Guid>> ConfirmUserAsync(string email, CancellationToken ct)
    {
        var identityUser =
            await userManager.FindByEmailAsync(email)
            ?? await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == email, ct);

        if (identityUser is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (identityUser.Email != null && identityUser.EmailConfirmed)
            return DomainErrors.IdentityErrors.DuplicatedConfirmation("Email already confirmed");

        if (identityUser.Email != null)
            identityUser.EmailConfirmed = true;

        var updateResult = await userManager.UpdateAsync(identityUser);

        if (!updateResult.Succeeded)
        {
            string errors = string.Join(" | ", updateResult.Errors.Select(e => e.Description));
            return DomainErrors.IdentityErrors.UpdateFailed(errors);
        }

        return identityUser.Id;
    }

    public async Task<ErrorOr<Success>> ResetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
        {
            return DomainErrors.IdentityErrors.UnverifiedAccount(
                "User must verify email or phone before resetting password"
            );
        }

        bool samePassword = await userManager.CheckPasswordAsync(user, newPassword);
        if (samePassword)
        {
            return DomainErrors.IdentityErrors.SamePassword(
                "New password cannot be the same as the current password"
            );
        }

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            return DomainErrors.IdentityErrors.PasswordResetFailed(
                "Failed to clear existing password"
            );
        }

        var addResult = await userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded)
        {
            if (
                addResult.Errors.Any(e =>
                    e.Code.Contains("PasswordTooShort") || e.Code.Contains("PasswordRequires")
                )
            )
            {
                return DomainErrors.IdentityErrors.WeakPassword(
                    "New password does not meet security requirements"
                );
            }

            return DomainErrors.IdentityErrors.PasswordResetFailed("Failed to set new password");
        }

        return new Success();
    }
}
