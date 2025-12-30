using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.ForgotPassword;

public record ResetPasswordOtpSendCommand(string Email) : IRequest<Result>;
