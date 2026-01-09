using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Family.Commands.UpdateFamily;

public sealed record UpdateFamilyCommand(
    Guid FamilyId,
    string? Name,
    string? FamilyBio
) : IRequest<Result>;
