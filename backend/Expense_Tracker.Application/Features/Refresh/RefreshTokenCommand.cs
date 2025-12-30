using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken, string DeviceId, string FcmToken)
    : IRequest<Result<AuthResponse>>;
