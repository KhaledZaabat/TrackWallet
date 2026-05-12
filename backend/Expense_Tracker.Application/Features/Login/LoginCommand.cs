using Expense_Tracker.Contracts.Reponses.Identity;

namespace Expense_Tracker.Application.Features.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string DeviceId,
    string FcmToken
);
