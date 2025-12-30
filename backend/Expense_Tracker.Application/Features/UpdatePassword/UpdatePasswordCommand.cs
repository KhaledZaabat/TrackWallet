using MediatR;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.UpdatePassword;

public record UpdatePasswordCommand(string CurrentPassword, string NewPassword, string UserIpAddress) : IRequest<Result>;
