using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Domain.Common.Identity;

public sealed partial class RefreshToken : Entity
{
    public byte[] TokenHash { get; private set; } = Array.Empty<byte>();
    public Guid UserId { get; private set; } = Guid.Empty;
    public string DeviceId { get; private set; } = string.Empty;
    public Guid SessionFamilyId { get; private set; } = Guid.Empty;
    public DateTimeOffset OriginalIssuedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken() { }

    private RefreshToken(
        Guid id,
        byte[] tokenHash,
        Guid userId,
        string deviceId,
        Guid sessionFamilyId,
        DateTimeOffset originalIssuedAt,
        DateTimeOffset expiresAt
    )
    {
        Id = id;
        TokenHash = tokenHash;
        UserId = userId;
        DeviceId = deviceId;
        SessionFamilyId = sessionFamilyId;
        OriginalIssuedAt = originalIssuedAt;
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    public static ErrorOr<RefreshToken> Create(
        byte[] tokenHash,
        Guid userId,
        string deviceId,
        Guid sessionFamilyId,
        DateTimeOffset originalIssuedAt,
        TimeSpan lifetime
    )
    {
        if (tokenHash is null || tokenHash.Length == 0)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "TokenHash is required."
            );

        if (userId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "UserId is required."
            );

        if (string.IsNullOrWhiteSpace(deviceId))
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "DeviceId is required."
            );

        if (sessionFamilyId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "SessionFamilyId is required."
            );

        if (lifetime <= TimeSpan.Zero)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "Lifetime must be greater than zero."
            );

        DateTimeOffset expires = DateTimeOffset.UtcNow.Add(lifetime);

        return new RefreshToken(
            Guid.CreateVersion7(),
            tokenHash,
            userId,
            deviceId,
            sessionFamilyId,
            originalIssuedAt,
            expires
        );
    }

    public ErrorOr<Success> Revoke()
    {
        if (IsExpired)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "Cannot revoke an expired refresh token."
            );

        if (IsRevoked)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "Refresh token is already revoked."
            );

        RevokedAt = DateTimeOffset.UtcNow;
        return new Success();
    }

    public ErrorOr<Success> MarkReplacedBy(Guid successorId)
    {
        if (successorId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "Successor token id is required."
            );

        if (IsRevoked)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(RefreshToken),
                "Refresh token is already revoked."
            );

        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenId = successorId;
        return new Success();
    }
}
