using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Application.Common.Errors;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Refresh.Dto;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Infrastructure.Data;

namespace Expense_Tracker.Domain.Common.Identity;

public sealed class RefreshTokenService(AppDbContext db, IIdentityService identityService, JwtSettings jwtSettings) : IRefreshTokenService
{

    public async Task<Result<AuthenticatedUser>> GetUserFromRefreshTokenAsync(
        string refreshToken,
        string deviceId,
        CancellationToken ct)
    {
        RefreshToken? dbToken = await db.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(rt =>
                rt.Token == refreshToken &&
                rt.DeviceId == deviceId,
                ct);

        if (dbToken is null || !dbToken.IsActive)
        {
            return Result.Failure<AuthenticatedUser>(
                TokenError.Invalid("Invalid or expired refresh token"));
        }

        dbToken.Revoke();
        await db.SaveChangesAsync(ct);

        return await identityService.GetUserByIdAsync(dbToken.UserId);
    }

    public async Task<Result<RefreshTokenDto>> GetLatestAsync(
        Guid userId,
        string deviceId,
        CancellationToken ct)
    {
        RefreshToken? entity = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (entity is null)
        {
            return Result.Failure<RefreshTokenDto>(
                TokenError.NotFound("Refresh token not found"));
        }

        RefreshTokenDto dto = new(
            entity.Token,
            entity.CreatedAt,
            entity.ExpiresAt,
            entity.RevokedAt is not null);

        return Result.Success(dto);
    }

    public async Task<Result> AddAsync(
        Guid userId,
        string token,
        string deviceId,
        CancellationToken ct)
    {
        Result<RefreshToken> createResult = RefreshToken.Create(
            token,
            userId,
            deviceId,
            TimeSpan.FromDays(jwtSettings.RefreshTokenExpirationDays));

        if (createResult.IsFailure)
            return Result.Failure(createResult.TryGetError());

        db.RefreshTokens.Add(createResult.TryGetValue());
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> RevokeActiveTokensAsync(
        Guid userId,
        string deviceId,
        CancellationToken ct)
    {
        List<RefreshToken> tokens = await db.RefreshTokens
            .AsTracking()
            .Where(rt =>
                rt.UserId == userId &&
                rt.DeviceId == deviceId &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(ct);

        foreach (RefreshToken token in tokens)
        {
            token.Revoke();
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }


    public async Task<Result> RevokeAsync(
        string token,
        CancellationToken ct)
    {
        RefreshToken? entity = await db.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

        if (entity is null)
        {
            return Result.Failure(
                TokenError.NotFound("Refresh token not found"));
        }

        entity.Revoke();
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
