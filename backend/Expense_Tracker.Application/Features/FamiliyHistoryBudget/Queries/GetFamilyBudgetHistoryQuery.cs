using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;

public sealed record GetFamilyBudgetHistoryQuery(
    Guid FamilyId,
    int Months = 1
) : IRequest<Result<List<BudgetHistoryItem>>>;
