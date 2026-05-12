namespace Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string Otp);
