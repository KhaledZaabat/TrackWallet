using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    ITokenProvider tokenProvider,
     IUserDeviceRepository userDeviceRepository,
     IAppDbContext db,
     IFamilyContext familyContext,
     IUserContext userContext
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

        FamilyContextDto? familyContextDto =
          familyContext.FamilyId is null
              ? null
              : await db.FamilyUsers
                  .Where(fu =>
                      fu.FamilyId == familyContext.FamilyId.Value &&
                      fu.UserId == userContext.UserId!.Value)
                  .Select(fu => new FamilyContextDto(
                      FamilyId: fu.Family.Id,
                      FamilyName: fu.Family.Name,
                      IsParent: fu.IsParent,
                      CurrentBudget: fu.Family.CurrentBudget
                  )).FirstOrDefaultAsync(cancellationToken);
        Result<AuthDto> tokenResult = await tokenProvider.GenerateJwtTokenWithFamilyAsync(user, request.DeviceId, familyContextDto, cancellationToken);
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