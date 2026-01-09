namespace Expense_Tracker.Domain.Common.ResultPattern.Error;

public sealed record DomainError(DomainErrorCode DomainErrorCode, string Type, string Description)
    : Error(DomainErrorCode, Type, Description)
{
    public static DomainError NotFound(string entity) =>
        new(DomainErrorCode.NotFound, $"Domain.{entity}.NotFound", $"{entity} not found.");

    public static DomainError Conflict(string entity) =>
        new(DomainErrorCode.Conflict, $"Domain.{entity}.Conflict", $"{entity} already exists.");

    public static DomainError InvalidState(string entity, string detail = "Invalid state") =>
        new(DomainErrorCode.InvalidState, $"Domain.{entity}.InvalidState", detail);

    public static DomainError Unexpected(string detail) =>
        new(DomainErrorCode.Unexpected, "Domain.Unexpected", detail);
    public static DomainError Forbidden(string detail) =>
     new(DomainErrorCode.Forbidden, "Domain.Forbidden", detail);

    public static DomainError Forbidden(string entity, string detail) =>
        new(DomainErrorCode.Forbidden, $"Domain.{entity}.Forbidden", detail);

    public static DomainError NotFound(string entity, string detail) =>
        new(DomainErrorCode.NotFound, $"Domain.{entity}.NotFound", detail);

    public static DomainError BusinessRule(string entity, string detail) =>
        new(DomainErrorCode.InvalidState, $"Domain.{entity}.BusinessRule", detail);
}
