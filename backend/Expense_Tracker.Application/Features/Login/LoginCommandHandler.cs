using ErrorOr;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;
using Expense_Tracker.Domain.Users;
using Wolverine;

namespace Expense_Tracker.Application.Features.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IRepository<User> users,
    IFileUrlResolver fileUrlResolver,
    IUserDeviceRepository userDeviceRepository)
{
    public async Task<ErrorOr<AuthCommandResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthenticatedUser> authResult =
            await identityService.AuthenticateAsync(
                request.EmailOrUserName,
                request.Password);

        if (authResult.IsError)
            return authResult.Errors;

        AuthenticatedUser authenticatedUser = authResult.Value;

        ErrorOr<AuthDto> tokenResult =
            await tokenProvider.GenerateJwtTokenAsync(
                authenticatedUser,
                request.DeviceId,
                cancellationToken);

        if (tokenResult.IsError)
            return tokenResult.Errors;

        AuthDto authDto = tokenResult.Value;
        Guid userId = Guid.Parse(authDto.UserId);

        User? user = await users.GetByIdAsync
            (userId, cancellationToken);
        
        if (user is null)
            return Error.NotFound("User not found");

        string? profileImageUrl = fileUrlResolver.GetUrl(user.ProfileImageFileId);




        MeResponse authResponse = new MeResponse(
            UserId: userId,
            Email: authDto.Email,
            UserName: user.UserName,
            FullName: user.FullName,
            BirthDate: user.BirthDate,
            IsMale: user.IsMale,
            ProfileImageUrl: profileImageUrl);

        await userDeviceRepository.UpsertAsync(
            userId,
            request.FcmToken,
            PushPlatform.Web,
            cancellationToken);

        return new AuthCommandResult(
            Response: authResponse,
            AccessToken: authDto.JwtToken.Token,
            AccessExpiresAt: new DateTimeOffset(authDto.JwtToken.ExpiresAt, TimeSpan.Zero),
            RefreshToken: authDto.RefreshToken.Token,
            RefreshExpiresAt: new DateTimeOffset(authDto.RefreshToken.ExpiresAt, TimeSpan.Zero));
    }
}