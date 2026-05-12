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
    private static byte[] Sha256(string raw) => SHA256.HashData(Encoding.UTF8.GetBytes(raw));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

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
            .RefreshTokens.AsNoTracking()
            .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId && rt.RevokedAt == null)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            return DomainErrors.TokenErrors.NotFound("Refresh token not found.");

        return new RefreshTokenDto(
            entity.Id.ToString(),
            entity.CreatedAt,
            entity.ExpiresAt,
            entity.RevokedAt is not null
        );
    }

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

        if (existing.ExpiresAt <= now)
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Expired("Refresh token has expired.");
        }

        if (
            now - existing.OriginalIssuedAt
            > TimeSpan.FromDays(jwtSettings.AbsoluteSessionLifetimeDays)
        )
        {
            await tx.RollbackAsync(ct);
            return DomainErrors.TokenErrors.Forbidden("Absolute session lifetime exceeded.");
        }

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

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException postgresEx
            && postgresEx.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}
