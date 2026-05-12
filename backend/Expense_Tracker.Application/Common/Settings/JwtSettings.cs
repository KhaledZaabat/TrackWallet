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

    [Range(0, 300, ErrorMessage = "Clock skew must be between 0 and 300 seconds.")]
    public int ClockSkewSeconds { get; set; } = 30;

    [Range(1, 60, ErrorMessage = "Silent refresh threshold must be between 1 and 60 minutes.")]
    public int SilentRefreshThresholdMinutes { get; set; } = 3;

    [Range(1, 3650, ErrorMessage = "Absolute session lifetime must be between 1 and 3650 days.")]
    public int AbsoluteSessionLifetimeDays { get; set; } = 180;

    [Range(1, 120, ErrorMessage = "Rotation grace must be between 1 and 120 seconds.")]
    public int RotationGraceSeconds { get; set; } = 10;

    public TimeSpan SilentRefreshThresholdAsTimeSpan
        => TimeSpan.FromMinutes(SilentRefreshThresholdMinutes);
}
