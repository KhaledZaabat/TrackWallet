using ErrorOr;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Identity;
using System.Security.Claims;

namespace Expense_Tracker.Application.Interfaces;

public interface ITokenProvider
{
    Task<ErrorOr<AuthDto>> GenerateJwtTokenAsync(
        AuthenticatedUser user,
        string deviceId,
        CancellationToken ct = default);

    Task<ErrorOr<AuthDto>> GenerateJwtTokenWithFamilyAsync(
        AuthenticatedUser user,
        string deviceId,
        FamilyContextDto? familyContext,
        CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Mints a fresh JWT access token with the same claim shape used by
    /// <see cref="GenerateJwtTokenWithFamilyAsync"/> but without any refresh-token
    /// persistence side effects. Consumed by <c>SilentRefreshMiddleware</c>
    /// for the rotation success path (R5.3, R17.4, R19.2).
    /// </summary>
    Task<AccessTokenResult> GenerateAccessTokenOnlyAsync(
        AuthenticatedUser user,
        FamilyContextDto? family,
        string deviceId,
        CancellationToken ct);
}

public readonly record struct AccessTokenResult(string Token, DateTimeOffset ExpiresAt);
