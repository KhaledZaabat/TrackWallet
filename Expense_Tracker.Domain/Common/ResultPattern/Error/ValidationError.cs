namespace Expense_Tracker.Domain.Common.ResultPattern.Error;

public sealed record ValidationError(DomainErrorCode DomainErrorCode, string Type, string Description)
    : Error(DomainErrorCode, Type, Description)
{
    public static ValidationError InvalidInput(string description = "Invalid input") =>
        new(DomainErrorCode.Validation, "Validation.InvalidInput", description);

    public static ValidationError MissingField(string fieldName) =>
        new(DomainErrorCode.Validation, "Validation.MissingField", $"The field '{fieldName}' is required.");

    public static ValidationError InvalidFormat(string fieldName, string description = "Invalid format") =>
        new(DomainErrorCode.Validation, "Validation.InvalidFormat", $"Field '{fieldName}': {description}");

    public static ValidationError ExceedsLimit(string fieldName, int maxLength) =>
        new(DomainErrorCode.Validation, "Validation.ExceedsLimit", $"Field '{fieldName}' exceeds {maxLength} characters.");

    public static ValidationError InvalidPhone(string fieldName = "Phone", string description = "Invalid phone number format") =>
        new(DomainErrorCode.Validation, "Validation.InvalidPhone", $"{fieldName}: {description}");
}
