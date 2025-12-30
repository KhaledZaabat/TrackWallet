using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string NewPassword, string UserIpAddress) : IRequest<Result>;
