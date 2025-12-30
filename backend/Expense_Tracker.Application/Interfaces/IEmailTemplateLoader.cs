namespace Expense_Tracker.Application.Interfaces;

public interface IEmailTemplateLoader : IScopedService
{
    Task<string> LoadTemplateAsync(string templateName, CancellationToken cancellationToken = default);
}