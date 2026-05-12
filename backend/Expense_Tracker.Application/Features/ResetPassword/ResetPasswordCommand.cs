namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string NewPassword, string UserIpAddress);
