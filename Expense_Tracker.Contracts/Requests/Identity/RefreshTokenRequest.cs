namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string ExpiredAccessToken,
    string DeviceId);
