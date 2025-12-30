using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Application.Common.Settings;

public sealed class OtpSettings
{
    public const string SectionName = "OtpSettings";

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ExpirationInSeconds must be greater than 0")]
    public int ExpirationInSeconds { get; init; }

    [Required]
    [Range(4, 8, ErrorMessage = "Digits must be between 4 and 8")]
    public int Digits { get; init; } = 4;
}