using ErrorOr;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Refresh.Dto;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;

public interface IRefreshTokenService : IScopedService
{
    Task<ErrorOr<AuthenticatedUser>> GetUserFromRefreshTokenAsync(
        string refreshToken,
        string deviceId,
        CancellationToken ct
    );

    Task<ErrorOr<RefreshTokenDto>> GetLatestAsync(
        Guid userId,
        string deviceId,
        CancellationToken ct
    );

    Task<ErrorOr<Success>> AddAsync(
        Guid userId,
        string token,
        string deviceId,
        CancellationToken ct = default
    );

    Task<ErrorOr<Success>> RevokeActiveTokensAsync(
        Guid userId,
        string deviceId,
        CancellationToken ct = default
    );

    Task<ErrorOr<Success>> RevokeAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Persists a fresh refresh-token session for a user/device with a brand-new <c>SessionFamilyId</c>.
    /// Stores only the SHA-256 hash of <paramref name="rawToken"/>; the raw value is never written to disk.
    /// Required for refresh rotation flow per design.md §Components → RefreshTokenService (R8.1, R8.2, R10.1, R10.2, R11.1).
    /// </summary>
    Task<ErrorOr<Success>> AddNewSessionAsync(
        Guid userId,
        string rawToken,
        string deviceId,
        Guid sessionFamilyId,
        DateTimeOffset originalIssuedAt,
        CancellationToken ct = default
    );

    /// <summary>
    /// Atomically rotates a presented refresh token: verifies the incoming raw value by hash,
    /// revokes the previous row, inserts a successor sharing the same <c>SessionFamilyId</c> and
    /// <c>OriginalIssuedAt</c>, and returns the new raw token alongside user/family context
    /// PLUS the <c>DeviceId</c> recovered from the persisted row (so callers don't have to
    /// track DeviceId separately).
    /// Single entry point for <c>SilentRefreshMiddleware</c> and the refresh endpoint per
    /// design.md §Components → RefreshTokenService (R8.1, R8.2, R10.1, R10.2, R11.1, R17.4).
    /// </summary>
    Task<ErrorOr<RotationSuccess>> RotateAsync(
        string rawIncomingToken,
        CancellationToken ct
    );

    /// <summary>
    /// Revokes every active refresh-token row for <paramref name="userId"/> across all devices.
    /// Backs the "logout everywhere" capability per design.md §Components → RefreshTokenService (R21.4).
    /// </summary>
    Task<ErrorOr<Success>> RevokeAllSessionsForUserAsync(
        Guid userId,
        CancellationToken ct = default
    );
}

public readonly record struct RotationSuccess(
    string NewRawToken,
    DateTimeOffset NewRefreshExpiresAt,
    Guid SessionFamilyId,
    DateTimeOffset OriginalIssuedAt,
    string DeviceId,
    AuthenticatedUser User,
    FamilyContextDto? Family
);
