using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Infrastructure.Common.Errors
{
    public sealed record ExternalAuthError(InfrastructureErrorCode InfrastructureErrorCode, string Type, string Description)
        : Error(InfrastructureErrorCode, Type, Description)
    {
        public static ExternalAuthError ProviderUnavailable(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "ExternalAuth.ProviderUnavailable", detail);

        public static ExternalAuthError InvalidProviderResponse(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "ExternalAuth.InvalidProviderResponse", detail);

        public static ExternalAuthError UserCreationFailed(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "ExternalAuth.UserCreationFailed", detail);

        public static ExternalAuthError LoginLinkFailed(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "ExternalAuth.LoginLinkFailed", detail);

        public static ExternalAuthError UserNotRegistered(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "ExternalAuth.UserNotRegistered", detail);

        public static ExternalAuthError Unknown(string detail) =>
            new(InfrastructureErrorCode.ExternalService, "ExternalAuth.Unknown", detail);
    }
}