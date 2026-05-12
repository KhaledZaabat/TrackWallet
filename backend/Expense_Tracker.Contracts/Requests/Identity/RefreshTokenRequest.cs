namespace Expense_Tracker.Contracts.Requests.Identity;

/// <summary>
/// Body for POST /api/identity/refresh. The raw refresh value comes from the HttpOnly
/// refresh cookie, and DeviceId is recovered from the persisted refresh-token row —
/// neither needs to be sent by the client.
/// </summary>
public sealed record RefreshTokenRequest(
    string FcmToken);
