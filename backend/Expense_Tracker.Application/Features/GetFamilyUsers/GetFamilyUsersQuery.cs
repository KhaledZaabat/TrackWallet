using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.GetFamilyUsers;

public sealed record GetFamilyUsersQuery()
    : IRequest<Result<List<FamilyUserSimpleResponse>>>;
