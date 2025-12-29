namespace Expense_Tracker.Domain.Common;



public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity()
    {
        Id = Guid.CreateVersion7();
    }

    protected Entity(Guid id)
    {

        Id = (id == Guid.Empty) ? Guid.CreateVersion7() : id;
    }


}


