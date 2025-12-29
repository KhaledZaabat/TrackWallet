namespace Expense_Tracker.Application.Constans;

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

    // Phone
    public const string PhoneRequired = "Phone number is required.";
    //   public const string InvalidPhone = "Invalid phone number format. Use E.164 (+1234567890)."; later
    public const string InvalidKuwaitiPhone = "Invalid Kuwaiti phone number. Must be 8 digits starting with 5, 6, or 9.";

    // Guest
    public const string DeviceIdRequired = "Device ID is Required.";
    public const string IdTokenRequired = "Google ID token is required.";

    public const string InvalidDeviceId = "Device ID is invalid.";


    public const string InvalidFcmToken = "Invalid FCM token.";


}


