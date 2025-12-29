namespace Expense_Tracker.Application.Interfaces;

public interface IUserContext
{
    Guid? UserId { get; }
}