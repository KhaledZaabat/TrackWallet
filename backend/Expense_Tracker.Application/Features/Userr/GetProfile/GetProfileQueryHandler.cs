using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.Userr.GetProfile;

public sealed class GetProfileQueryHandler(
    IAppDbContext db,
    IUserContext userContext,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder)
    : IRequestHandler<GetProfileQuery, Result<UserProfileResponse>>
{
    public async Task<Result<UserProfileResponse>> Handle(
        GetProfileQuery query,
        CancellationToken ct)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
            return Result.Failure<UserProfileResponse>(UserError.NotFound());

        UserProfileResponse? profile = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileResponse(
                u.Id,
                u.FullName,
                u.UserName,
                u.Email,
                u.BirthDate,
                u.IsMale,
                u.ProfileImageFileId.HasValue
                    ? fileUrlBuilder.GetUrl(u.ProfileImageFileId.Value)
                    : null,
                u.NotificationPreferences.EmailNotifications,
                u.NotificationPreferences.PushNotifications
            ))
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return Result.Failure<UserProfileResponse>(UserError.NotFound());

        return Result.Success(profile);
    }
}
