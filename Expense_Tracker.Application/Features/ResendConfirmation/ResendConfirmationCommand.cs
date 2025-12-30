using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResendConfirmation;

public record ResendConfirmationCommand(string Email) : IRequest<Result>;
