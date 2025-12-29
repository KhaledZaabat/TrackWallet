using MediatR;

namespace Expense_Tracker.Application.Common;

public record ApplicationEvent() : INotification;
