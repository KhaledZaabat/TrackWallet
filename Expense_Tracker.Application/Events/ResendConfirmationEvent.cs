using Expense_Tracker.Application.Common;
using Expense_Tracker.Application.Dtos;

namespace Expense_Tracker.Application.Events;

public sealed record ResendConfirmationEvent(AuthenticatedUser User) : ApplicationEvent;
