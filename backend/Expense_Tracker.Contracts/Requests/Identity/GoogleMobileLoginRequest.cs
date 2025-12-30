namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record GoogleMobileLoginRequest(string IdToken, string DeviceId, string FcmToken);
