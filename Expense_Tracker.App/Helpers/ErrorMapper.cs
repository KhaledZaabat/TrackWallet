using Expense_Tracker.Application.Common.Errors;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Infrastructure.Common.Errors;

namespace Expense_Tracker.App.Helpers;

public static class ErrorMapper
{
    public static int ToHttpStatus(this Error error)
    {
        switch (error.Code)
        {

            case DomainErrorCode domain:
                if (ReferenceEquals(domain, DomainErrorCode.NotFound))
                    return StatusCodes.Status404NotFound;

                if (ReferenceEquals(domain, DomainErrorCode.Conflict))
                    return StatusCodes.Status409Conflict;

                if (ReferenceEquals(domain, DomainErrorCode.InvalidState))
                    return StatusCodes.Status409Conflict;

                if (ReferenceEquals(domain, DomainErrorCode.Validation))
                    return StatusCodes.Status400BadRequest;

                if (ReferenceEquals(domain, DomainErrorCode.Unexpected))
                    return StatusCodes.Status500InternalServerError;


                if (ReferenceEquals(domain, DomainErrorCode.Forbidden))
                    return StatusCodes.Status403Forbidden;

                if (ReferenceEquals(domain, DomainErrorCode.Unauthorized))
                    return StatusCodes.Status401Unauthorized;


                return StatusCodes.Status500InternalServerError;


            case ApplicationErrorCode app:
                if (ReferenceEquals(app, ApplicationErrorCode.Validation))
                    return StatusCodes.Status400BadRequest;

                if (ReferenceEquals(app, ApplicationErrorCode.BadRequest))
                    return StatusCodes.Status400BadRequest;

                if (ReferenceEquals(app, ApplicationErrorCode.Unauthorized))
                    return StatusCodes.Status401Unauthorized;

                if (ReferenceEquals(app, ApplicationErrorCode.Forbidden))
                    return StatusCodes.Status403Forbidden;

                if (ReferenceEquals(app, ApplicationErrorCode.Conflict))
                    return StatusCodes.Status409Conflict;

                if (ReferenceEquals(app, ApplicationErrorCode.NotFound))
                    return StatusCodes.Status404NotFound;

                return StatusCodes.Status500InternalServerError;


            case InfrastructureErrorCode infra:
                if (ReferenceEquals(infra, InfrastructureErrorCode.Validation))
                    return StatusCodes.Status400BadRequest;
                if (ReferenceEquals(infra, InfrastructureErrorCode.Authentication))
                    return StatusCodes.Status401Unauthorized;
                if (ReferenceEquals(infra, InfrastructureErrorCode.Conflict))
                    return StatusCodes.Status409Conflict;
                if (ReferenceEquals(infra, InfrastructureErrorCode.Database))
                    return StatusCodes.Status500InternalServerError;
                if (ReferenceEquals(infra, InfrastructureErrorCode.ExternalService))
                    return StatusCodes.Status502BadGateway;
                if (ReferenceEquals(infra, InfrastructureErrorCode.ServiceUnavailable))
                    return StatusCodes.Status503ServiceUnavailable;
                if (ReferenceEquals(infra, InfrastructureErrorCode.Timeout))
                    return StatusCodes.Status504GatewayTimeout;
                if (ReferenceEquals(infra, InfrastructureErrorCode.NotFound))
                    return StatusCodes.Status404NotFound;
                if (ReferenceEquals(infra, InfrastructureErrorCode.Forbidden))
                    return StatusCodes.Status403Forbidden;
                return StatusCodes.Status500InternalServerError;


            default:
                return StatusCodes.Status500InternalServerError;
        }
    }
}
