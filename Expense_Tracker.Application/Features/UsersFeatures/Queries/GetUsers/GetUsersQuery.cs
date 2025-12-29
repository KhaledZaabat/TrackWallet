using MediatR;
using Expense_Tracker.Contracts.Requests.Users;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Users.Abstraction;

namespace Expense_Tracker.Application.Features.UsersFeatures.Queries.GetUsers;


public sealed record GetUsersQuery(Role? Role)
    : IRequest<Result<IReadOnlyList<UserListItemDto>>>;
