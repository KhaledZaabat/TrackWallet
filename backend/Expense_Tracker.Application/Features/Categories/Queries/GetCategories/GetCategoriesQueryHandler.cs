using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetCategoriesQuery, Result<List<CategoryResponse>>>
{
    public async Task<Result<List<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        List<Category> categories = await db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        List<CategoryResponse> response = categories.Adapt<List<CategoryResponse>>();

        return Result.Success(response);
    }
}