
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.PushNotifications.Enums;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.FCM;

public sealed class UserDeviceRepository(IAppDbContext db) : IUserDeviceRepository, IScopedService
{


    public async Task UpsertAsync(
        Guid userId,
        string token,
        PushPlatform platform,
        CancellationToken cancellationToken)
    {
        UserDevice? device =
            await db.UserDevices
                .SingleOrDefaultAsync(
                    x => x.DeviceToken == token,
                    cancellationToken);

        if (device != null)
        {
            device.BindToUser(userId);
            device.Touch();
            return;
        }

        UserDevice newDevice = UserDevice.Create(token, platform);
        newDevice.BindToUser(userId);

        await db.UserDevices.AddAsync(newDevice, cancellationToken);
    }

    public async Task UnbindDeviceAsync(
    string token,
    CancellationToken cancellationToken)
    {
        UserDevice? device =
            await db.UserDevices
                .SingleOrDefaultAsync(
                    x => x.DeviceToken == token,
                    cancellationToken);

        if (device is null)
            return;

        device.UnbindUser();
    }

    public async Task ClearUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<UserDevice> devices =
            await db.UserDevices
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);

        foreach (UserDevice device in devices)
        {
            device.UnbindUser();
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await db.UserDevices
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.DeviceToken)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveTokenAsync(
        string token,
        CancellationToken cancellationToken)
    {
        UserDevice? device =
            await db.UserDevices
                .SingleOrDefaultAsync(
                    x => x.DeviceToken == token,
                    cancellationToken);

        if (device != null)
        {
            db.UserDevices.Remove(device);
        }
    }
}








