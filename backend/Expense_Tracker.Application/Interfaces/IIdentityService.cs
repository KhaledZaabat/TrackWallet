using ErrorOr;
using Expense_Tracker.Application.Dtos;

namespace Expense_Tracker.Application.Interfaces;

public interface IIdentityService : IScopedService
{
    Task<bool> IsInRoleAsync(Guid userId, string role);

    /// <summary>
    /// Authenticates a user by email or username + password. Resolves the
    /// identity by email first; falls back to username lookup if the input
    /// is not a registered email. Same downstream checks
    /// (email-confirmed, password match) regardless of which identifier
    /// was used.
    /// </summary>
    Task<ErrorOr<AuthenticatedUser>> AuthenticateAsync(string emailOrUserName, string password);

    Task<ErrorOr<AuthenticatedUser>> GetUserByIdAsync(Guid userId);

    Task<ErrorOr<string>> GetFullNameAsync(Guid userId);

    Task<ErrorOr<IdentityRegistrationResult>> CreateIdentityByEmailAsync(string email, string password, string userName, CancellationToken cancellationToken);
    Task<ErrorOr<Success>> ChangePasswordAsync(Guid userId, string currentPassword, string NewPassword);

    Task<ErrorOr<AuthenticatedUser>> FindUserByEmailAsync(string email, bool requireConfirmedEmail = true);

    Task<ErrorOr<Success>> IsUserConfirmedAsync(string emailOrPhone, CancellationToken ct);
    Task<ErrorOr<Success>> IsUserNotConfirmedAsync(string emailOrPhone, CancellationToken ct);

    /// <summary>
    /// Generates an email-confirmation token for the user identified by
    /// <paramref name="email"/>. The token is HMAC-protected by ASP.NET Identity
    /// (DataProtector + SecurityStamp), needs no server-side storage, and is
    /// safe to embed in a URL.
    /// </summary>
    Task<ErrorOr<string>> GenerateEmailConfirmationTokenAsync(string email);

    /// <summary>
    /// Validates the supplied email-confirmation token and flips the user's
    /// <c>EmailConfirmed</c> flag. Returns the user id on success.
    /// </summary>
    Task<ErrorOr<Guid>> ConfirmEmailWithTokenAsync(string email, string token, CancellationToken ct);

    /// <summary>
    /// Generates a password-reset token. The token is invalidated automatically
    /// on the next sensitive change (password change, email change, etc.) via
    /// the user's <c>SecurityStamp</c>.
    /// </summary>
    Task<ErrorOr<string>> GeneratePasswordResetTokenAsync(string email);

    /// <summary>
    /// Validates the supplied reset token and sets <paramref name="newPassword"/>.
    /// On success the user's <c>SecurityStamp</c> rotates, invalidating any other
    /// outstanding reset tokens.
    /// </summary>
    Task<ErrorOr<Success>> ResetPasswordWithTokenAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken);
}
