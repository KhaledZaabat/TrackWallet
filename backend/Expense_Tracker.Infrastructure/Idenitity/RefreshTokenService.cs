using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Refresh.Dto;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Expense_Tracker.Domain.Common.Identity;

public sealed class RefreshTokenService(
    AppDbContext db,
    IIdentityService identityService,
    JwtSettings jwtSettings,
    ILogger<RefreshTokenService> logger
) : IRefreshTokenService
{
    /// <summary>
    /// Hashes a raw refresh-token string with SHA-256 so only the digest is persisted (R18.5).
    /// </summary>
    private static byte[] Sha256(string raw) => SHA256.HashData(Encoding.UTF8.GetBytes(raw));

    /// <summary>
    /// Base64-url encodes a byte buffer without padding, matching the CSPRNG token format used
    /// elsewhere in the auth pipeline (R18.3).
    /// </summary>
    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>
    /// Validates a raw refresh token, applies the same reuse-detection and family-wide
    /// revocation as <see cref="RotateAsync"/>, and returns the owning user.
    /// Implements R7.2, R7.3, R9.2, R10.1, R10.2, R10.4, R18.4, R21.2.
    /// </summary>
    public async Task<ErrorOr<AuthenticatedUser>> GetUserFromRefreshTokenAsync(
        string refreshToken,
        string deviceId,
        CancellationToken ct
    )
    {
        byte[] hash = Sha256(refreshToken);

        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted,
            ct
        );

        // Row-level lock prevents concurrent reuse (R7.2, R7.3, R9.2).
        RefreshToken? dbToken = await db
            .RefreshTokens.FromSqlInterpolated(
                $@"
                SELECT * FROM ""RefreshTokens""
                WHERE ""TokenHash"" = {hash}
                  AND ""DeviceId"" = {deviceId}
                FOR UPDATE"
            )
            .AsTracking()
            .FirstOrDefaultAsync(ct);

        if (dbToken is null)
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Invalid("Invalid refresh token.");
        }

        // Reuse detection: a revoked token being presented means the session was stolen.
        // Revoke the entire family before returning (R10.1, R10.2, R10.4, R18.4, R21.2).
        if (dbToken.RevokedAt is not null)
        {
            await RevokeSessionFamilyAsync(dbToken.SessionFamilyId, dbToken.DeviceId, ct);
            await tx.CommitAsync(ct);

            logger.LogWarning(
                "Refresh-token reuse detected. UserId={UserId} DeviceId={DeviceId} SessionFamilyId={SessionFamilyId}",
                dbToken.UserId,
                dbToken.DeviceId,
                dbToken.SessionFamilyId
            );

            return DomainErrors.TokenErrors.ReuseDetected();
        }

        if (!dbToken.IsActive)
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Expired("Refresh token has expired.");
        }

        dbToken.Revoke();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await identityService.GetUserByIdAsync(dbToken.UserId);
    }

    public async Task<ErrorOr<RefreshTokenDto>> GetLatestAsync(
        Guid userId,
        string deviceId,
        CancellationToken ct
    )
    {
        RefreshToken? entity = await db
            .RefreshTokens.AsNoTracking() // Read-only; no change tracking needed.
            .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId && rt.RevokedAt == null)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            return DomainErrors.TokenErrors.NotFound("Refresh token not found.");

        // Never expose the SHA-256 digest to any client surface.
        // Use the opaque entity Id for internal tracing only.
        return new RefreshTokenDto(
            entity.Id.ToString(),
            entity.CreatedAt,
            entity.ExpiresAt,
            entity.RevokedAt is not null
        );
    }

    // Collapsed public entry-points into a single private implementation (issue #6).
    public Task<ErrorOr<Success>> AddAsync(
        Guid userId,
        string token,
        string deviceId,
        CancellationToken ct
    ) => AddInternalAsync(userId, token, deviceId, null, null, ct);

    public Task<ErrorOr<Success>> AddNewSessionAsync(
        Guid userId,
        string rawToken,
        string deviceId,
        Guid sessionFamilyId,
        DateTimeOffset originalIssuedAt,
        CancellationToken ct = default
    ) => AddInternalAsync(userId, rawToken, deviceId, sessionFamilyId, originalIssuedAt, ct);

    // Single UPDATE — no materialization, no per-row round-trips (issue #3).
    public async Task<ErrorOr<Success>> RevokeActiveTokensAsync(
        Guid userId,
        string deviceId,
        CancellationToken ct = default
    )
    {
        await db
            .RefreshTokens.Where(rt =>
                rt.UserId == userId
                && rt.DeviceId == deviceId
                && rt.RevokedAt == null
                && rt.ExpiresAt > DateTimeOffset.UtcNow
            )
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);

        return new Success();
    }

    // Same transaction + FOR UPDATE pattern as RotateAsync (issue #2).
    public async Task<ErrorOr<Success>> RevokeAsync(string token, CancellationToken ct = default)
    {
        byte[] hash = Sha256(token);

        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted,
            ct
        );

        RefreshToken? entity = await db
            .RefreshTokens.FromSqlInterpolated(
                $@"
                SELECT * FROM ""RefreshTokens""
                WHERE ""TokenHash"" = {hash}
                FOR UPDATE"
            )
            .AsTracking()
            .FirstOrDefaultAsync(ct);

        if (entity is null)
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.NotFound("Refresh token not found.");
        }

        // Idempotent: already-revoked tokens are a no-op, not an error.
        if (entity.RevokedAt is not null)
        {
            await tx.RollbackAsync(ct);
            return new Success();
        }

        entity.Revoke();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new Success();
    }

    /// <summary>
    /// Atomically rotates a presented refresh token under a row-locked transaction:
    /// verifies the incoming raw value by hash, detects reuse of an already-revoked row,
    /// enforces absolute session lifetime, revokes the old row while linking it to the
    /// freshly-minted successor, and returns the raw successor plus user/family context.
    ///
    /// User and family context are resolved INSIDE the transaction so that any failure
    /// can still be rolled back, leaving the client in a consistent state.
    ///
    /// Implements R7.2, R7.3, R8.1, R8.2, R9.2, R9.3, R10.1, R10.2, R10.4,
    /// R11.1–R11.4, R18.2, R18.4, R21.1, R21.2.
    /// </summary>
    public async Task<ErrorOr<RotationSuccess>> RotateAsync(
        string rawIncomingToken,
        CancellationToken ct
    )
    {
        byte[] incomingHash = Sha256(rawIncomingToken);

        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted,
            ct
        );

        // Single indexed SELECT ... FOR UPDATE against the unique TokenHash index (R7.2, R7.3, R9.2).
        // TokenHash is unique across the table — no need for a DeviceId predicate; the stored
        // row is itself the authoritative source of the device binding.
        RefreshToken? existing = await db
            .RefreshTokens.FromSqlInterpolated(
                $@"
                SELECT * FROM ""RefreshTokens""
                WHERE ""TokenHash"" = {incomingHash}
                FOR UPDATE"
            )
            .AsTracking()
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Invalid("Invalid refresh token.");
        }

        // Reuse detection (R10.1, R10.2, R10.4, R18.4, R21.2):
        // a revoked row being presented means the token was stolen — nuke the whole family.
        if (existing.RevokedAt is not null)
        {
            await RevokeSessionFamilyAsync(existing.SessionFamilyId, existing.DeviceId, ct);
            await tx.CommitAsync(ct);

            logger.LogWarning(
                "Refresh-token reuse detected. UserId={UserId} DeviceId={DeviceId} SessionFamilyId={SessionFamilyId}",
                existing.UserId,
                existing.DeviceId,
                existing.SessionFamilyId
            );

            return DomainErrors.TokenErrors.ReuseDetected();
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Expired row — middleware treats this as a silent-refresh failure.
        if (existing.ExpiresAt <= now)
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Expired("Refresh token has expired.");
        }

        // Absolute-lifetime enforcement (R11.2, R11.3).
        if (
            now - existing.OriginalIssuedAt
            > TimeSpan.FromDays(jwtSettings.AbsoluteSessionLifetimeDays)
        )
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Forbidden("Absolute session lifetime exceeded.");
        }

        // ── Resolve user + family BEFORE minting or committing ───────────────
        // If either lookup fails we can still roll back cleanly, keeping the
        // client's current token valid. Previously this happened post-commit,
        // which would leave the client with a revoked token and no successor.
        ErrorOr<AuthenticatedUser> userResult = await identityService.GetUserByIdAsync(
            existing.UserId
        );
        if (userResult.IsError)
        {
            await tx.RollbackAsync(ct);
            return userResult.Errors;
        }

        FamilyContextDto? family = await db
            .FamilyUsers.AsNoTracking()
            .Where(fu => fu.UserId == existing.UserId)
            .OrderBy(fu => fu.JoinedAtUtc)
            .Join(
                db.Families.AsNoTracking(),
                fu => fu.FamilyId,
                f => f.Id,
                (fu, f) => new FamilyContextDto(f.Id, f.Name, fu.IsParent, f.CurrentBudget)
            )
            .FirstOrDefaultAsync(ct);

        // ── Mint the successor (R18.2, R18.3) ───────────────────────────────
        byte[] newRawBytes = RandomNumberGenerator.GetBytes(32);
        string newRawToken = Base64UrlEncode(newRawBytes);
        byte[] newHash = Sha256(newRawToken);

        ErrorOr<RefreshToken> createResult = RefreshToken.Create(
            newHash,
            existing.UserId,
            existing.DeviceId,
            existing.SessionFamilyId,
            existing.OriginalIssuedAt,
            TimeSpan.FromDays(jwtSettings.RefreshTokenExpirationDays)
        );

        if (createResult.IsError)
        {
            await tx.RollbackAsync(ct);
            return createResult.Errors;
        }

        RefreshToken successor = createResult.Value;

        // Atomic: revoke old row → insert successor → commit (R8.1, R8.2, R9.3, R11.1, R11.4).
        ErrorOr<Success> markResult = existing.MarkReplacedBy(successor.Id);
        if (markResult.IsError)
        {
            await tx.RollbackAsync(ct);
            return markResult.Errors;
        }

        db.RefreshTokens.Add(successor);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new RotationSuccess(
            NewRawToken: newRawToken,
            NewRefreshExpiresAt: successor.ExpiresAt,
            SessionFamilyId: successor.SessionFamilyId,
            OriginalIssuedAt: successor.OriginalIssuedAt,
            DeviceId: existing.DeviceId,
            User: userResult.Value,
            Family: family
        );
    }

    public async Task<ErrorOr<Success>> RevokeAllSessionsForUserAsync(
        Guid userId,
        CancellationToken ct = default
    )
    {
        await db
            .RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(rt => rt.RevokedAt, _ => DateTimeOffset.UtcNow),
                ct
            );

        return new Success();
    }

    // ─────────────────────────────────────────────────────────────
    //  Private helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Bulk-revokes all active tokens in a session family for a given device.
    /// Called both by <see cref="RotateAsync"/> and <see cref="GetUserFromRefreshTokenAsync"/>
    /// on reuse detection so the behaviour is identical across both paths.
    /// </summary>
    private Task RevokeSessionFamilyAsync(
        Guid sessionFamilyId,
        string deviceId,
        CancellationToken ct
    ) =>
        db
            .RefreshTokens.Where(rt =>
                rt.SessionFamilyId == sessionFamilyId
                && rt.DeviceId == deviceId
                && rt.RevokedAt == null
            )
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);

    private async Task<ErrorOr<Success>> AddInternalAsync(
        Guid userId,
        string token,
        string deviceId,
        Guid? sessionFamilyId,
        DateTimeOffset? originalIssuedAt,
        CancellationToken ct
    )
    {
        byte[] hash = Sha256(token);

        ErrorOr<RefreshToken> createResult = RefreshToken.Create(
            hash,
            userId,
            deviceId,
            sessionFamilyId ?? Guid.CreateVersion7(),
            originalIssuedAt ?? DateTimeOffset.UtcNow,
            TimeSpan.FromDays(jwtSettings.RefreshTokenExpirationDays)
        );

        if (createResult.IsError)
            return createResult.Errors;

        db.RefreshTokens.Add(createResult.Value);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Extremely rare hash collision or duplicate submission — surface as a clean error
            // rather than letting the raw DB exception bubble up to the caller.
            logger.LogError(
                ex,
                "Unique constraint violation persisting refresh token. UserId={UserId} DeviceId={DeviceId}",
                userId,
                deviceId
            );

            return DomainErrors.TokenErrors.Conflict("A token conflict occurred. Please retry.");
        }

        return new Success();
    }

    /// <summary>
    /// Heuristic check for unique-constraint violations across Npgsql and other providers.
    /// Avoids a hard dependency on provider-specific exception types.
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException postgresEx
            && postgresEx.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
