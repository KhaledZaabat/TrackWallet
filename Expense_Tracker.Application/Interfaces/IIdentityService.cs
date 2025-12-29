
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Interfaces;

public interface IIdentityService : IScopedService
{
    Task<bool> IsInRoleAsync(Guid userId, string role);


    Task<Result<AuthenticatedUser>> AuthenticateByEmailAsync(string email, string password);


    Task<Result<AuthenticatedUser>> GetUserByIdAsync(Guid userId);

    Task<Result<string>> GetFullNameAsync(Guid userId);


    Task<Result<IdentityRegistrationResult>> CreateIdentityByEmailAsync(string email, string FullName, string Role, CancellationToken cancellationToken);
    public Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string NewPassword);


    Task<Result<AuthenticatedUser>> FindUserByEmailAsync(string email);

    public Task<Result> ResetPasswordWithCodeAsync(Guid userId, string code, string newPassword, CancellationToken cancellationToken);
    public Task<Result> IsUserConfirmedAsync(string emailOrPhone, CancellationToken ct);
    public Task<Result> IsUserNotConfirmedAsync(string emailOrPhone, CancellationToken ct);

    public Task<Result<string>> GeneratePasswordResetCodeAsync(Guid userId, CancellationToken ct);


}
