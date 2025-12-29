using MediatR;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.ForgotPassword;

public record class ForgetPasswordCommand(string Email) : IRequest<Result>;
