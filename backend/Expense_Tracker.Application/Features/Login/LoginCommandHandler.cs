using ErrorOr;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features;
using Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;
using Wolverine;

namespace Expense_Tracker.Application.Features.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IRepository<User> users,
    IFileUrlResolver fileUrlResolver,
    IRepository<UserDevice> userDevices,
    IMessageBus bus,
    IUserDeviceRepository userDeviceRepository
)
{
    public async Task<ErrorOr<AuthCommandResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthenticatedUser> authResult =
            await identityService.AuthenticateByEmailAsync(
                request.Email,
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

        Guid? profileFileId = await users.Query()
            .Where(u => u.Id == userId)
            .Select(u => u.ProfileImageFileId)
            .FirstOrDefaultAsync(cancellationToken);

        string? profileImageUrl = fileUrlResolver.GetUrl(profileFileId);

        ErrorOr<List<FamilyResponse>> familiesResult =
            await bus.InvokeAsync<ErrorOr<List<FamilyResponse>>>(
                new GetUserFamiliesQuery(userId), cancellationToken);

        if (familiesResult.IsError)
            return familiesResult.Errors;

        List<FamilyResponse>? families = familiesResult.Value;

        AuthResponse authResponse = (authDto, profileImageUrl, families).Adapt<AuthResponse>();

        await userDeviceRepository.UpsertAsync(userId, request.FcmToken,Domain.PushNotifications.Enums.PushPlatform.Web, cancellationToken);

        return new AuthCommandResult(
            Response: authResponse,
            AccessToken: authDto.JwtToken.Token,
            AccessExpiresAt: new DateTimeOffset(authDto.JwtToken.ExpiresAt, TimeSpan.Zero),
            RefreshToken: authDto.RefreshToken.Token,
            RefreshExpiresAt: new DateTimeOffset(authDto.RefreshToken.ExpiresAt, TimeSpan.Zero));
    }
}
