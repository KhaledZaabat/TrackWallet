using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Domain.Events;

public sealed record InvitationCancelledEvent(Domain.Invitation.Invitation invitation) : DomainEvent;
