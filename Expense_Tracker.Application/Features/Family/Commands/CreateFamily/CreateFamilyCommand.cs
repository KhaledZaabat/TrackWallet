using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Family.Commands.CreateFamily;

public sealed record CreateFamilyCommand(
    Guid UserId,
    string Name,
    decimal InitialBudget,
    string? FamilyBio
) : IRequest<Result<CreateFamilyResponse>>;
