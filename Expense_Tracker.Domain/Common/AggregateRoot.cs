using Dodo.Primitives;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base() { }
    protected AggregateRoot(Guid id) : base(id) { }

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}