namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record LogoutRequest(string DeviceId, string FcmToken);
