namespace Expense_Tracker.Application.Dtos;

public record AuthenticatedUser(Guid Id, string Email, string FullName, string Role);
