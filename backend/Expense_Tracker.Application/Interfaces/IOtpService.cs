namespace Expense_Tracker.Application.Interfaces;

public interface IOtpService
{
    string Generate(string key, int digits = 6);
    bool Validate(string key, string otp, bool removeOnSuccess = true);
    void Remove(string key);
    public bool Exists(string key);
}