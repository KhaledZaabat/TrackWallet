using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Domain.CategoryFolder;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(IRepository<Category> categories)
{
    public async Task<ErrorOr<List<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        List<Category> list = await categories.Query()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        List<CategoryResponse> response = list.Adapt<List<CategoryResponse>>();

        return response;
    }
}
