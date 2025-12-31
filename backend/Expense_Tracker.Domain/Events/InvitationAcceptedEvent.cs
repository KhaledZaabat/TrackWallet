using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Domain.Events;

public sealed record InvitationAcceptedEvent(Domain.Invitation.Invitation Invitation) : DomainEvent;
