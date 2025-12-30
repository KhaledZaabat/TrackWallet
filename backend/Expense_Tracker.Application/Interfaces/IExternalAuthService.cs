using Expense_Tracker.Application.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Interfaces;

public interface IExternalAuthService : IScopedService
{
    Task<Result<ExternalAuthDto>> SignInWithGoogleIdTokenAsync(string idToken, CancellationToken ct); // mobile
}