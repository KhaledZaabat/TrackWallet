namespace Expense_Tracker.Contracts.Requests.Identity;


public sealed record LoginRequest(
    string Email,
    string Password,
    string DeviceId,
    string FcmToken);


