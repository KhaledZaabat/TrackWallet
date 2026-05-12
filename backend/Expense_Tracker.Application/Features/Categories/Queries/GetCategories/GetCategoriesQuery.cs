using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;

namespace Expense_Tracker.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery() : ICachedQuery<ErrorOr<List<CategoryResponse>>>
{
    public string CacheKey => "categories:all";
    public TimeSpan Expiration => TimeSpan.FromDays(7); // Categories rarely change
}
