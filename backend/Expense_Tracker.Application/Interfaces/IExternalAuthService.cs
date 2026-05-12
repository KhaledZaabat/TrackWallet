using ErrorOr;
using Expense_Tracker.Application.Common;

namespace Expense_Tracker.Application.Interfaces;

public interface IExternalAuthService : IScopedService
{
    Task<ErrorOr<ExternalAuthDto>> SignInWithGoogleIdTokenAsync(string idToken, CancellationToken ct);
}
