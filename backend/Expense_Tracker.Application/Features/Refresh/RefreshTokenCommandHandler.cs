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
     IAppDbContext db,
     IFamilyContext familyContext
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

        FamilyContextDto? familyContextDto = (familyContext.FamilyId != null) ? db.Families
      .Where(f => f.Id == familyContext.FamilyId)
      .Select(f => new FamilyContextDto(
          FamilyId: f.Id,
          FamilyName: f.Name,
          IsParent: familyContext.IsParent,
          CurrentBudget: f.CurrentBudget
      ))
      .FirstOrDefault() : null;
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