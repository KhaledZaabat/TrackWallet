namespace Expense_Tracker.Application.Interfaces;

public interface IDbInitializer : IScopedService
{
    /// <summary>
    /// Seeds roles, default users, and role permissions.
    /// Must be called once at startup.
    /// </summary>
    Task SeedAsync();
}
