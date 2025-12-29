using MediatR;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.Login;


public sealed record LoginCommand(
    string Email,
    string Password,
    string DeviceId
) : IRequest<Result<AuthResponse>>;
