using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Infrastructure.Common.Errors
{
    public sealed record SystemError(InfrastructureErrorCode InfrastructureErrorCode, string Type, string Description)
        : Error(InfrastructureErrorCode, Type, Description)
    {
        public static SystemError Database(string detail) =>
            new(InfrastructureErrorCode.Database, "System.Database", detail);

        public static SystemError Timeout(string detail) =>
            new(InfrastructureErrorCode.Timeout, "System.Timeout", detail);

        public static SystemError ExternalService(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "System.ExternalService", detail);
    }
}