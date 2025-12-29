
using Expense_Tracker.Domain.Common.ResultPattern.Error;
namespace Expense_Tracker.Application.Common.Errors;

public sealed record ValidationError(ApplicationErrorCode ApplicationErrorCode, string Type, string Description)
    : Error(ApplicationErrorCode, Type, Description)
{
    public static readonly ValidationError None = new(
        ApplicationErrorCode.None,
        "Validation.None",
        string.Empty);

    public static ValidationError InvalidInput(string description = "Invalid input") =>
        new(ApplicationErrorCode.Validation,
            "Validation.InvalidInput",
            description);

    public static ValidationError MissingField(string fieldName) =>
        new(ApplicationErrorCode.Validation,
            "Validation.MissingField",
            $"The field '{fieldName}' is required.");

    public static ValidationError InvalidFormat(string fieldName, string description = "Invalid format") =>
        new(ApplicationErrorCode.Validation,
            "Validation.InvalidFormat",
            $"Field '{fieldName}': {description}");

    public static ValidationError ExceedsLimit(string fieldName, int maxLength) =>
        new(ApplicationErrorCode.Validation,
            "Validation.ExceedsLimit",
            $"Field '{fieldName}' exceeds {maxLength} characters.");

    public static ValidationError InvalidPhone(string fieldName = "Phone", string description = "Invalid phone number format") =>
        new(ApplicationErrorCode.Validation,
            "Validation.InvalidPhone",
            $"{fieldName}: {description}");
}