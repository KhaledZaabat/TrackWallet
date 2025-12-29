namespace Makayen.Application.Constans;

public static class ValidationMessages
{
    // Common
    public const string Required = "This field is required.";
    public const string InvalidFormat = "The provided format is invalid.";
    public const string TooShort = "The value is too short.";
    public const string TooLong = "The value is too long.";

    // Email
    public const string InvalidEmail = "Invalid email address.";
    public const string EmailRequired = "Email is required.";

    // Password
    public const string PasswordRequired = "Password is required.";
    public const string WeakPassword = "Password must contain upper, lower, number, and special character.";
    public const string PasswordTooShort = "Password must be at least 8 characters.";



}


