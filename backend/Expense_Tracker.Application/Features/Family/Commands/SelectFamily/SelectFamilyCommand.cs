using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

public sealed record SelectFamilyCommand(
    Guid UserId,
    Guid FamilyId,
    string DeviceId
) : IRequest<Result<SelectFamilyResponse>>;
