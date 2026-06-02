using Expense_Tracker.Contracts.Reponses.Identity;

namespace Expense_Tracker.Application.Features;

/// <summary>
/// Internal per-request result carrying the cookie-less <see cref="AuthResponse"/>
/// body alongside the raw access + refresh token values and their expiries. Consumed
/// only by the controller layer so <see cref="AuthResponse"/> itself stays free of
/// token material (R1.1, R1.3, R14.3, R15.5).
/// </summary>
public sealed record AuthCommandResult(
    MeResponse Response,
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt);
