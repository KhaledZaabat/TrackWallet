using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using ErrorOr;
using Expense_Tracker.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.PushNotifications.UpdateFcmToken;

public sealed class UpsertUserDeviceCommandHandler(
    IUserContext userContext,
    IRepository<UserDevice> userDevices
)
{
    public async Task<ErrorOr<Success>> Handle(
        UpsertUserDeviceCommand request,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;

        if (userId is null)
            return DomainErrors.UserErrors.Unauthorized();

        UserDevice? device = await userDevices.QueryTracked()
            .SingleOrDefaultAsync(x => x.DeviceToken == request.FcmToken, cancellationToken);

        if (device != null)
        {
            device.BindToUser(userId.Value);
            device.Touch();
            return new Success();
        }

        UserDevice newDevice = UserDevice.Create(
            request.FcmToken,
            Domain.PushNotifications.Enums.PushPlatform.Android);
        newDevice.BindToUser(userId.Value);
        await userDevices.AddAsync(newDevice, cancellationToken);

        return new Success();
    }
}
