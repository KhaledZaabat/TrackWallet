using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Domain.Events;

public sealed record InvitationCreatedEvent(Domain.Invitation.Invitation Invitation) : DomainEvent;
