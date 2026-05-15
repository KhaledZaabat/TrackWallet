namespace Expense_Tracker.Application.Interfaces;

public interface IFileUrlResolver : ISingletonService
{
    string? GetUrl(Guid? id);
}
