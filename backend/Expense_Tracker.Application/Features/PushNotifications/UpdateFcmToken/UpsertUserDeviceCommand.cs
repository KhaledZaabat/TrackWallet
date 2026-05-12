namespace Expense_Tracker.Application.Features.PushNotifications.UpdateFcmToken;

public sealed record UpsertUserDeviceCommand(
    string FcmToken);
