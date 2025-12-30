
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Interfaces;

public interface IIdentityService : IScopedService
{
    Task<bool> IsInRoleAsync(Guid userId, string role);


    Task<Result<AuthenticatedUser>> AuthenticateByEmailAsync(string email, string password);

    public Task<Result<Guid>> ConfirmUserAsync(string email, CancellationToken ct);
    Task<Result<AuthenticatedUser>> GetUserByIdAsync(Guid userId);

    Task<Result<string>> GetFullNameAsync(Guid userId);


    Task<Result<IdentityRegistrationResult>> CreateIdentityByEmailAsync(string email, string password, string userName, CancellationToken cancellationToken);
    public Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string NewPassword);


    Task<Result<AuthenticatedUser>> FindUserByEmailAsync(string email, bool requireConfirmedEmail = true);

    public Task<Result> IsUserConfirmedAsync(string emailOrPhone, CancellationToken ct);
    public Task<Result> IsUserNotConfirmedAsync(string emailOrPhone, CancellationToken ct);

    public Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);


}
