using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Family.Commands.DeleteFamily;

public sealed record DeleteFamilyCommand(
    Guid FamilyId,
    Guid RequestingUserId
) : IRequest<Result>;
