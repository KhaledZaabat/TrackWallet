namespace Expense_Tracker.Application.Features.Login;

/// <summary>
/// Login command. <see cref="EmailOrUserName"/> accepts either a registered
/// email address or a username — the Identity layer resolves whichever one
/// matches.
/// </summary>
public sealed record LoginCommand(
    string EmailOrUserName,
    string Password,
    string DeviceId,
    string FcmToken);
