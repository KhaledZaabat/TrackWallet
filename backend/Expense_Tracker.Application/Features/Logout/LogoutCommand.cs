using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
namespace Expense_Tracker.Application.Features.Identity.Commands.Logout;

public sealed record LogoutCommand(string DeviceId, string FcmToken) : IRequest<Result>;
