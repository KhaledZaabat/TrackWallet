namespace Makayen.Application.Constans;

public static class ValidationLimits
{
    //  Authentication
    public const int PasswordMinLength = 8;      // Minimum password strength requirement
    public const int PasswordMaxLength = 128;    // Security-safe upper limit for hashing libraries

    //  Email
    public const int EmailMaxLength = 254;       // RFC 5321-compliant maximum length

    //  Phone
    public const int PhoneNumberMaxLength = 15;  // E.164 standard (+1234567890...)

    //  Device / Guest

    //  Tokens
    public const int AccessTokenMaxLength = 4096;    // Safe upper bound for long JWTs
    public const int RefreshTokenMaxLength = 512;    // Usually a GUID or secure random string

    //  User profile
    public const int FullNameMaxLength = 50;
}
