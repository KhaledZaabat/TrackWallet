using Expense_Tracker.Contracts.Reponses.Identity;

public interface IAuthCookieWriter
{
    void Write(AuthResponse auth);
}