using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.CategoryFolder;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Data.Seeding;

public class DbInitializer(IAppDbContext context) : IDbInitializer
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

        // Seed all category types
        foreach (CategoryType categoryType in Enum.GetValues<CategoryType>())
        {
            var categoryResult = Category.Create(categoryType);
            if (categoryResult.IsSuccess)
            {
                categories.Add(categoryResult.TryGetValue());
            }
        }

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}