using System.Text;
using ErrorOr;
using Expense_Tracker.Application.Constans;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Helpers;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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

    public async Task<ErrorOr<string>> GenerateEmailConfirmationTokenAsync(string email)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        string raw = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return EncodeTokenForUrl(raw);
    }

    public async Task<ErrorOr<Guid>> ConfirmEmailWithTokenAsync(
        string email,
        string token,
        CancellationToken ct
    )
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (user.EmailConfirmed)
            return DomainErrors.IdentityErrors.DuplicatedConfirmation("Email already confirmed");

        if (!TryDecodeTokenFromUrl(token, out string decoded))
            return DomainErrors.IdentityErrors.InvalidToken();

        IdentityResult result = await userManager.ConfirmEmailAsync(user, decoded);
        if (!result.Succeeded)
        {
            return DomainErrors.IdentityErrors.InvalidToken();
        }

        return user.Id;
    }

    public async Task<ErrorOr<string>> GeneratePasswordResetTokenAsync(string email)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        string raw = await userManager.GeneratePasswordResetTokenAsync(user);
        return EncodeTokenForUrl(raw);
    }

    public async Task<ErrorOr<Success>> ResetPasswordWithTokenAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return DomainErrors.IdentityErrors.NotFound();

        if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
            return DomainErrors.IdentityErrors.UnverifiedAccount(
                "User must verify email or phone before resetting password"
            );

        if (!TryDecodeTokenFromUrl(token, out string decoded))
            return DomainErrors.IdentityErrors.InvalidToken();

        IdentityResult result = await userManager.ResetPasswordAsync(user, decoded, newPassword);

        if (result.Succeeded)
            return new Success();

        if (result.Errors.Any(e => e.Code == "InvalidToken"))
            return DomainErrors.IdentityErrors.InvalidToken();

        if (
            result.Errors.Any(e =>
                e.Code.Contains("PasswordTooShort") || e.Code.Contains("PasswordRequires")
            )
        )
        {
            return DomainErrors.IdentityErrors.WeakPassword(
                "New password does not meet security requirements"
            );
        }

        return DomainErrors.IdentityErrors.PasswordResetFailed(
            string.Join(" | ", result.Errors.Select(e => e.Description))
        );
    }

   
    private static string EncodeTokenForUrl(string token) =>
        WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

 
    private static bool TryDecodeTokenFromUrl(string urlToken, out string decoded)
    {
        decoded = string.Empty;
        if (string.IsNullOrEmpty(urlToken)) return false;

        try
        {
            decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(urlToken));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
