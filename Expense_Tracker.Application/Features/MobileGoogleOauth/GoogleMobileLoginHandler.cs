using Expense_Tracker.Application.Common;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.External_Providers.Commands.MobileGoogleOauth;

public sealed class GoogleMobileLoginHandler(
    IExternalAuthService externalAuth,
    IAppDbContext db,
    ITokenProvider tokenProvider,
    IUserDeviceRepository userDeviceRepository,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
) : IRequestHandler<GoogleMobileLoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        GoogleMobileLoginCommand request,
        CancellationToken ct)
    {
        Result<ExternalAuthDto> authResult =
            await externalAuth.SignInWithGoogleIdTokenAsync(request.IdToken, ct);

        if (authResult.IsFailure)
            return Result.Failure<AuthResponse>(authResult.TryGetError());

        ExternalAuthDto external = authResult.TryGetValue();

        var domainUser = await db.Users
            .FirstOrDefaultAsync(u => u.Id == external.IdentityId, ct);

        if (domainUser is null)
        {
            var userResult = Domain.Users.User.Create(
                id: external.IdentityId,
                fullName: $"{external.FirstName} {external.LastName}" ?? "Google",
                userName: external.UserName ?? "User",
                email: external.Email,
                fireEvent: false
            );

            if (userResult.IsFailure)
                return Result.Failure<AuthResponse>(userResult.TryGetError());

            domainUser = userResult.TryGetValue();
            db.Users.Add(domainUser);
            await db.SaveChangesAsync(ct);
        }

        var authenticatedUser = new AuthenticatedUser(
            Id: external.IdentityId,
            Email: external.Email,
            UserName: external.UserName!
        );

        var jwtResult = await tokenProvider.GenerateJwtTokenAsync(
            authenticatedUser,
            request.DeviceId,
            ct);

        if (jwtResult.IsFailure)
            return Result.Failure<AuthResponse>(jwtResult.TryGetError());

        AuthDto authTokens = jwtResult.TryGetValue();

        Guid? profileFileId = await db.Users
            .Where(u => u.Id == external.IdentityId)
            .Select(u => u.ProfileImageFileId)
            .FirstOrDefaultAsync(ct);

        string? profileImageUrl = fileUrlBuilder.GetUrl(profileFileId);

        AuthResponse authResponse = (authTokens, profileImageUrl).Adapt<AuthResponse>();

        await userDeviceRepository.UpsertAsync(
            external.IdentityId,
            request.FcmToken,
            platform: Domain.PushNotifications.Enums.PushPlatform.Android,
            ct);

        await db.SaveChangesAsync(ct);

        return Result.Success(authResponse);
    }
}