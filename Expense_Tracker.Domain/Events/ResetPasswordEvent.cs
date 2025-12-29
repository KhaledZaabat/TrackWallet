using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Domain.Events;

public record ResetPasswordEvent(Guid userId, string Email, string FullName, string Role) : DomainEvent;
