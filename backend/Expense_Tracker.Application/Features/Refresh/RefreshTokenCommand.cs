namespace Expense_Tracker.Application.Features.Refresh;

public sealed record RefreshTokenCommand(
    string RawRefreshToken,
    string FcmToken);
