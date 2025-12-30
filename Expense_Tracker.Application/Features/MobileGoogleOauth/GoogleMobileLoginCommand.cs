
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.External_Providers.Commands.MobileGoogleOauth;

public sealed record GoogleMobileLoginCommand(string IdToken, string DeviceId, string FcmToken)
    : IRequest<Result<AuthResponse>>;
