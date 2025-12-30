using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IAppDbContext db,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
    IUserDeviceRepository userDeviceRepository
) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> authResult =
            await identityService.AuthenticateByEmailAsync(
                request.Email,
                request.Password);

        if (authResult.IsFailure)
            return Result.Failure<AuthResponse>(authResult.TryGetError());

        AuthenticatedUser authenticatedUser = authResult.TryGetValue();

        Result<AuthDto> tokenResult =
            await tokenProvider.GenerateJwtTokenAsync(
                authenticatedUser,
                request.DeviceId,
                cancellationToken);

        if (tokenResult.IsFailure)
            return Result.Failure<AuthResponse>(tokenResult.TryGetError());
        AuthDto authDto = tokenResult.TryGetValue();
        Guid? profileFileId = await db.Users.Where(u => u.Id == authenticatedUser.Id).Select(u => u.ProfileImageFileId).FirstOrDefaultAsync();
        string? ProfileImageUrl = fileUrlBuilder.GetUrl(profileFileId);
        AuthResponse authResponse = (authDto, ProfileImageUrl).Adapt<AuthResponse>();

        Guid userId = Guid.Parse(authDto.UserId);

        await userDeviceRepository.UpsertAsync(userId,
                                        request.FcmToken,
                                        platform: Domain.PushNotifications.Enums.PushPlatform.Android,
                                        cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(authResponse);
    }
}
