using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.PushNotifications.UpdateFcmToken;

public sealed record UpsertUserDeviceCommand(
    string FcmToken) : IRequest<Result>;
