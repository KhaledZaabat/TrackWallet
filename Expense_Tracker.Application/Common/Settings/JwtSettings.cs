using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Application.Common.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters long for security.")]
    public string SecretKey { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Access token expiration must be between 1 and 1440 minutes.")]
    public int AccessTokenExpirationMinutes { get; set; }

    [Range(1, 365, ErrorMessage = "Refresh token expiration must be between 1 and 365 days.")]
    public int RefreshTokenExpirationDays { get; set; } = 90;
}