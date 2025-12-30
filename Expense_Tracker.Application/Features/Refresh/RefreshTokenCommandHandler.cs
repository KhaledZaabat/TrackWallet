using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
using MediatR;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    ITokenProvider tokenProvider,
     IUserDeviceRepository userDeviceRepository,
     IAppDbContext db
) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>


{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userResult = await refreshTokenService.GetUserFromRefreshTokenAsync(
            request.RefreshToken,
            request.DeviceId,
            cancellationToken
        );

        if (userResult.IsFailure)
            return Result.Failure<AuthResponse>(userResult.TryGetError());

        var user = userResult.TryGetValue();

        Result<AuthDto> tokenResult = await tokenProvider.GenerateJwtTokenAsync(user, request.DeviceId, cancellationToken);
        AuthResponse response = tokenResult.TryGetValue().Adapt<AuthResponse>();

        Guid userId = Guid.Parse(response.UserId);
        await userDeviceRepository.UpsertAsync(userId,
                                  request.FcmToken,
                                  platform: Domain.PushNotifications.Enums.PushPlatform.Android,
                                  cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AuthResponse>(response);
    }
}