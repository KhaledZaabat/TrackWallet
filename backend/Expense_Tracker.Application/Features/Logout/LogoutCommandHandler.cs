using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenService refreshTokens,
    IUserContext userContext,
    IRepository<UserDevice> userDevices,
    IFcmTopicService topicService)
{
    public async Task<ErrorOr<Success>> Handle(LogoutCommand request, CancellationToken ct)
    {
        Guid? userId = userContext.UserId;

        if (userId is null)
            return DomainErrors.UserErrors.Unauthorized();

        UserDevice? device = await userDevices.QueryTracked()
            .SingleOrDefaultAsync(x => x.DeviceToken == request.FcmToken, ct);

        if (device is not null)
        {
            foreach (var topic in device.SubscribedTopics.ToList())
            {
                await topicService.UnsubscribeFromTopicAsync(
                    new[] { device.DeviceToken },
                    topic,
                    ct);
            }

            device.UnbindUser();
        }

        return await refreshTokens.RevokeActiveTokensAsync(userId.Value, request.DeviceId, ct);
    }
}
