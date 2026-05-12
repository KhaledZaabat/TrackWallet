using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Data.Seeding;

public class DbInitializer(AppDbContext context) : IDbInitializer
{
    public async Task SeedAsync()
    {
        await SeedCategoriesAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>();

        foreach (CategoryType categoryType in Enum.GetValues<CategoryType>())
        {
            var categoryResult = Category.Create(categoryType);
            if (!categoryResult.IsError)
            {
                categories.Add(categoryResult.Value);
            }
        }

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
