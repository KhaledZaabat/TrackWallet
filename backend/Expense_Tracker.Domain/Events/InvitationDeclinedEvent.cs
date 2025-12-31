using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Domain.Events;

public sealed record InvitationDeclinedEvent(Domain.Invitation.Invitation Invitation) : DomainEvent;
