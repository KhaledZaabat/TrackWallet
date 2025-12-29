using MediatR;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken, string DeviceId)
    : IRequest<Result<AuthResponse>>;
