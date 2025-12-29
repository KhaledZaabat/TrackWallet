using System.Text.RegularExpressions;

namespace Makayen.Application.Constans;

public static class ValidationPatterns
{
    // Email pattern (RFC 5322 simplified)
    public const string Email = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";




    // Strong password: at least 8 chars, one upper, one lower, one digit, one special
    public const string StrongPassword =
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$";

    public static bool IsEmail(string value) =>
           Regex.IsMatch(value, Email);



}

