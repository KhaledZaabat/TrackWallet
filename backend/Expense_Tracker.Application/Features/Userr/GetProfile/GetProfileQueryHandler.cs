using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Userr.GetProfile;

public sealed class GetProfileQueryHandler(
    IRepository<User> userRepo,
    IUserContext userContext,
    IFileUrlResolver fileUrlResolver)
{
    public async Task<ErrorOr<UserProfileResponse>> Handle(
        GetProfileQuery query,
        CancellationToken ct)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
            return DomainErrors.UserErrors.NotFound();

        UserProfileResponse? profile = await userRepo.QueryTracked()
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileResponse(
                u.Id,
                u.FullName,
                u.UserName,
                u.Email,
                u.BirthDate,
                u.IsMale,
                u.ProfileImageFileId.HasValue
                    ? fileUrlResolver.GetUrl(u.ProfileImageFileId.Value)
                    : null,
                u.NotificationPreferences.EmailNotifications,
                u.NotificationPreferences.PushNotifications
            ))
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            return DomainErrors.UserErrors.NotFound();

        return profile;
    }
}
