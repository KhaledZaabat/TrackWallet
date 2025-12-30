namespace Expense_Tracker.Contracts.Requests.PushNotifications;

public sealed record UpsertUserDeviceRequest(
    string FcmToken);