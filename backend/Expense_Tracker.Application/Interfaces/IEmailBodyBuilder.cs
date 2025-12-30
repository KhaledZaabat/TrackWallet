namespace Expense_Tracker.Application.Interfaces;

public interface IEmailBodyBuilder : IScopedService

{
    string Build(
        string templateHtml,
        IReadOnlyDictionary<string, string> model);
}
