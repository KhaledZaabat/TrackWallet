namespace Expense_Tracker.Contracts.Requests.Identity;

/// <summary>
/// Body for <c>POST /api/identity/login</c>. <see cref="EmailOrUserName"/>
/// accepts either a registered email address or a username — the server
/// resolves whichever matches.
/// </summary>
public sealed record LoginRequest(
    string EmailOrUserName,
    string Password,
    string DeviceId="test",
    string FcmToken="test");
