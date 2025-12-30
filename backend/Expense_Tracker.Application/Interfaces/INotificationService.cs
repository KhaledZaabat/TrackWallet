namespace Expense_Tracker.Application.Interfaces;

public interface INotificationService : IScopedService
{
    Task SendEmailAsync(string to, string subject, string htmBody, CancellationToken cancellationToken = default);
}
