using ErrorOr;
using Expense_Tracker.Application.Dtos;

namespace Expense_Tracker.Application.Interfaces;

public interface IIdentityService : IScopedService
{
    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<ErrorOr<AuthenticatedUser>> AuthenticateByEmailAsync(string email, string password);

    Task<ErrorOr<Guid>> ConfirmUserAsync(string email, CancellationToken ct);
    Task<ErrorOr<AuthenticatedUser>> GetUserByIdAsync(Guid userId);

    Task<ErrorOr<string>> GetFullNameAsync(Guid userId);

    Task<ErrorOr<IdentityRegistrationResult>> CreateIdentityByEmailAsync(string email, string password, string userName, CancellationToken cancellationToken);
    Task<ErrorOr<Success>> ChangePasswordAsync(Guid userId, string currentPassword, string NewPassword);

    Task<ErrorOr<AuthenticatedUser>> FindUserByEmailAsync(string email, bool requireConfirmedEmail = true);

    Task<ErrorOr<Success>> IsUserConfirmedAsync(string emailOrPhone, CancellationToken ct);
    Task<ErrorOr<Success>> IsUserNotConfirmedAsync(string emailOrPhone, CancellationToken ct);

    Task<ErrorOr<Success>> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);
}
