namespace Expense_Tracker.Application.Features.Refresh;

/// <summary>
/// Refresh command. The raw refresh value is supplied by the controller directly
/// from the refresh cookie — it is never read from the request body (R15.1, R15.2, R15.4).
/// DeviceId is recovered from the persisted refresh-token row inside the rotation
/// transaction, so it does not need to be sent by the client.
/// </summary>
public sealed record RefreshTokenCommand(
    string RawRefreshToken,
    string FcmToken);
