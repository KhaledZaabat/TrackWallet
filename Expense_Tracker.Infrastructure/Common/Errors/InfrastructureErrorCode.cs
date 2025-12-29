using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Infrastructure.Common.Errors
{
    public sealed record InfrastructureErrorCode : ErrorCode
    {
        private InfrastructureErrorCode(string value) : base(value) { }

        public static readonly InfrastructureErrorCode None =
            new("infra.none");

        // Database errors - 500
        public static readonly InfrastructureErrorCode Database =
            new("infra.database_error");

        // Network/Service errors - 502/503/504
        public static readonly InfrastructureErrorCode Timeout =
            new("infra.timeout");

        public static readonly InfrastructureErrorCode ExternalService =
            new("infra.external_service_error");

        public static readonly InfrastructureErrorCode ServiceUnavailable =
            new("infra.unavailable");

        // Client errors - 400/401
        public static readonly InfrastructureErrorCode Validation =
            new("infra.validation_error");

        public static readonly InfrastructureErrorCode Authentication =
            new("infra.authentication_error");

        public static readonly InfrastructureErrorCode Conflict =
            new("infra.conflict_error");
        public static readonly InfrastructureErrorCode NotFound =
        new("infra.not_found");


        public static readonly InfrastructureErrorCode Forbidden =
        new("infra.forbidden");


    }
}