using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery() : IRequest<Result<List<CategoryResponse>>>, ICachedQuery<Result<List<CategoryResponse>>>
{
    public string CacheKey => "categories:all";
    public TimeSpan Expiration => TimeSpan.FromDays(7); // Categories rarely change
}
