using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Infrastructure.Common.Errors
{
    public sealed record ApplicationRoleError(
        InfrastructureErrorCode InfrastructureErrorCode,
        string Type,
        string Description)
        : Error(InfrastructureErrorCode, Type, Description)
    {
        public static ApplicationRoleError NotFound(string description = "Role not found") =>
            new(InfrastructureErrorCode.NotFound, "ApplicationRoleError.NotFound", description);

        public static ApplicationRoleError InvalidPermissions(string description = "Invalid permissions") =>
            new(InfrastructureErrorCode.Validation, "ApplicationRoleError.InvalidPermissions", description);

        public static ApplicationRoleError DuplicatedRole(string description = "Another role with the same name already exists") =>
            new(InfrastructureErrorCode.Conflict, "ApplicationRoleError.DuplicatedRole", description);

        public static ApplicationRoleError CreationFailed(string description = "Failed to create role") =>
            new(InfrastructureErrorCode.Validation, "ApplicationRoleError.CreationFailed", description);

        public static ApplicationRoleError UpdateFailed(string description = "Failed to update role") =>
            new(InfrastructureErrorCode.Validation, "ApplicationRoleError.UpdateFailed", description);
    }
}