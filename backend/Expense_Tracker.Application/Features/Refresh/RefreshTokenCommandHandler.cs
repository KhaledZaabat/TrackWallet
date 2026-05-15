using ErrorOr;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    ITokenProvider tokenProvider,
    IRepository<User> users,
    IFileUrlResolver fileUrlResolver,
    IRepository<UserDevice> userDevices,
    IMessageBus bus)
{
    public async Task<ErrorOr<AuthCommandResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Atomic rotation: verifies the raw refresh by SHA-256 hash, detects reuse,
        //    enforces absolute lifetime, and issues a successor sharing the same
        //    SessionFamilyId / OriginalIssuedAt / DeviceId recovered from the persisted
        //    row (R9.3, R11.1, R15.4).
        ErrorOr<RotationSuccess> rotation = await refreshTokenService.RotateAsync(
            request.RawRefreshToken,
            cancellationToken);

        if (rotation.IsError)
            return rotation.Errors;

        RotationSuccess success = rotation.Value;

        // 2. Mint a fresh access token with the rotated principal's claim shape (R5.3).
        AccessTokenResult access = await tokenProvider.GenerateAccessTokenOnlyAsync(
            success.User,
            success.Family,
            success.DeviceId,
            cancellationToken);

        Guid userId = success.User.Id;

        // 3. Profile image URL for the response body.
        Guid? profileFileId = await users.Query()
            .Where(u => u.Id == userId)
            .Select(u => u.ProfileImageFileId)
            .FirstOrDefaultAsync(cancellationToken);

        string? profileImageUrl = fileUrlResolver.GetUrl(profileFileId);

        // 4. User's families for the response body (same source the login handler uses).
        ErrorOr<List<FamilyResponse>> familiesResult =
            await bus.InvokeAsync<ErrorOr<List<FamilyResponse>>>(
                new GetUserFamiliesQuery(userId), cancellationToken);

        if (familiesResult.IsError)
            return familiesResult.Errors;

        // 5. Build cookie-less AuthResponse — no token material in the body (R1.1, R1.3, R15.5).
        AuthResponse response = new(
            UserId: userId.ToString(),
            Email: success.User.Email,
            FullName: success.User.UserName,
            Families: familiesResult.Value,
            ProfileImageUrl: profileImageUrl);

        // 6. Device upsert — preserve existing push-notification behavior.
        UserDevice? device = await userDevices.QueryTracked()
            .SingleOrDefaultAsync(x => x.DeviceToken == request.FcmToken, cancellationToken);

        if (device is not null)
        {
            device.BindToUser(userId);
            device.Touch();
        }
        else
        {
            UserDevice newDevice = UserDevice.Create(
                request.FcmToken,
                Domain.PushNotifications.Enums.PushPlatform.Android);
            newDevice.BindToUser(userId);
            await userDevices.AddAsync(newDevice, cancellationToken);
        }

        // 7. Hand raw tokens to the controller layer for cookie writing (R15.4).
        return new AuthCommandResult(
            Response: response,
            AccessToken: access.Token,
            AccessExpiresAt: access.ExpiresAt,
            RefreshToken: success.NewRawToken,
            RefreshExpiresAt: success.NewRefreshExpiresAt);
    }
}
