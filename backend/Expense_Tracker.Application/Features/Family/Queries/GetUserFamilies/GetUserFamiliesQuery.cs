using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;

public sealed record GetUserFamiliesQuery(Guid userId) : IRequest<Result<List<FamilyResponse>>>;
