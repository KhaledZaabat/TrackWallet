
namespace Expense_Tracker.Application.Features.Identity.Commands.Logout;

public sealed record LogoutCommand(string DeviceId, string FcmToken);
