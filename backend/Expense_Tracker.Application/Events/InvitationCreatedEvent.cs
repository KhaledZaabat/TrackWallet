using Expense_Tracker.Domain.Invitation;

namespace Expense_Tracker.Application.Events;

public sealed record InvitationCreatedEvent(Invitation Invitation);
