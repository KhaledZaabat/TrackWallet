using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Interfaces;

public interface IExternalAuthService : IScopedService
{
    Task<Result<AuthenticatedUser>> SignInWithGoogleAsync(CancellationToken ct);
}