using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Login;


public sealed record LoginCommand(
    string Email,
    string Password,
    string DeviceId,
    string FcmToken
) : IRequest<Result<AuthResponse>>;
