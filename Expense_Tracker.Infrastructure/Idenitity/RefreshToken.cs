using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;


namespace Expense_Tracker.Domain.Common.Identity;


public sealed partial class RefreshToken : Entity
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; } = Guid.Empty;

    public string DeviceId { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken() { }

    private RefreshToken(
        Guid id,
        string token,
        Guid userId,
        string deviceId,
        DateTimeOffset expiresAt)
    {
        Id = id;
        Token = token;
        UserId = userId;
        DeviceId = deviceId;
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    public static Result<RefreshToken> Create(
        string token,
        Guid userId,
        string deviceId,
        TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<RefreshToken>(
                DomainError.InvalidState(nameof(RefreshToken), "Token is required."));
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure<RefreshToken>(
                DomainError.InvalidState(nameof(RefreshToken), "UserId is required."));
        }

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Result.Failure<RefreshToken>(
                DomainError.InvalidState(nameof(RefreshToken), "DeviceId is required."));
        }

        if (lifetime <= TimeSpan.Zero)
        {
            return Result.Failure<RefreshToken>(
                DomainError.InvalidState(nameof(RefreshToken), "Lifetime must be greater than zero."));
        }

        DateTimeOffset expires = DateTimeOffset.UtcNow.Add(lifetime);

        return Result.Success(
            new RefreshToken(
                Guid.CreateVersion7(),
                token,
                userId,
                deviceId,
                expires
            ));
    }

    public Result Revoke()
    {
        if (IsExpired)
        {
            return Result.Failure(
                DomainError.InvalidState(nameof(RefreshToken), "Cannot revoke an expired refresh token."));
        }

        if (IsRevoked)
        {
            return Result.Failure(
                DomainError.InvalidState(nameof(RefreshToken), "Refresh token is already revoked."));
        }

        RevokedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
