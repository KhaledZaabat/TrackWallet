namespace Expense_Tracker.Application.Interfaces;

public interface IUrlBuilder
{
    string? GetUrl(Guid? id);
}