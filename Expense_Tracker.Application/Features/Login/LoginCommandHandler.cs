using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
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
    IUserDeviceRepository userDeviceRepository,
    ISender sender
) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Authenticate user
        Result<AuthenticatedUser> authResult =
            await identityService.AuthenticateByEmailAsync(
                request.Email,
                request.Password);

        if (authResult.IsFailure)
            return Result.Failure<AuthResponse>(authResult.TryGetError());

        AuthenticatedUser authenticatedUser = authResult.TryGetValue();

        // 2. Generate JWT tokens
        Result<AuthDto> tokenResult =
            await tokenProvider.GenerateJwtTokenAsync(
                authenticatedUser,
                request.DeviceId,
                cancellationToken);

        if (tokenResult.IsFailure)
            return Result.Failure<AuthResponse>(tokenResult.TryGetError());

        AuthDto authDto = tokenResult.TryGetValue();
        Guid userId = Guid.Parse(authDto.UserId);

        // 3. Get user profile image
        Guid? profileFileId = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.ProfileImageFileId)
            .FirstOrDefaultAsync(cancellationToken);

        string? profileImageUrl = fileUrlBuilder.GetUrl(profileFileId);

        // 4. Get user's first family (or null if no families)
        Result<List<FamilyResponse>> familiesResult =
            await sender.Send(new GetUserFamiliesQuery(userId), cancellationToken);

        if (familiesResult.IsFailure)
            return Result.Failure<AuthResponse>(familiesResult.TryGetError());

        List<FamilyResponse>? families = familiesResult.TryGetValue();


        // 5. Map to AuthResponse
        AuthResponse authResponse = (authDto, profileImageUrl, families).Adapt<AuthResponse>();

        // 6. Upsert user device for push notifications
        await userDeviceRepository.UpsertAsync(
            userId,
            request.FcmToken,
            platform: Domain.PushNotifications.Enums.PushPlatform.Android,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(authResponse);
    }
}