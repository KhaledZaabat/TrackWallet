using MediatR;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.ResetPassword
{
    public sealed record ResetPasswordWithCodeCommand(
        Guid UserId,
        string Code,
        string NewPassword,
        string UserIpAddress
    ) : IRequest<Result>;
}