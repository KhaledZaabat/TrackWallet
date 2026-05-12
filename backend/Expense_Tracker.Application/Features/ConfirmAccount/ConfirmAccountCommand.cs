namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

public sealed record ConfirmAccountCommand(string Email, string Otp);
