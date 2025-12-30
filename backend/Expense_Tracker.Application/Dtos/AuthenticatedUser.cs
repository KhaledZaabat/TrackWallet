namespace Expense_Tracker.Application.Dtos;

public record AuthenticatedUser(Guid Id, string Email, string UserName, string? Role = null);
