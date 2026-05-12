namespace Expense_Tracker.Application.Features.UpdatePassword;

public record UpdatePasswordCommand(string CurrentPassword, string NewPassword, string UserIpAddress);
