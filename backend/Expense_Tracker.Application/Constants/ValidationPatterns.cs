using System.Text.RegularExpressions;

namespace Expense_Tracker.Application.Constans;

public static class ValidationPatterns
{
    // Email pattern (RFC 5322 simplified).
    public const string Email = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    // Numeric OTP — kept for any internal flows that still use codes.
    public const string Otp = "^[0-9]+$";

    // Strong password: at least 8 chars, one upper, one lower, one digit, one special.
    public const string StrongPassword =
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$";

    /// <summary>
    /// Username: must start with a letter, then 2–19 letters, digits, or
    /// underscores. Total length 3–20. Mirrors the SPA regex
    /// <c>/^[a-zA-Z][a-zA-Z0-9_]{2,19}$/</c>.
    /// </summary>
    public const string UserName = @"^[a-zA-Z][a-zA-Z0-9_]{2,19}$";

    public static bool IsEmail(string value) => Regex.IsMatch(value, Email);

    public static bool IsUserName(string value) => Regex.IsMatch(value, UserName);
}
