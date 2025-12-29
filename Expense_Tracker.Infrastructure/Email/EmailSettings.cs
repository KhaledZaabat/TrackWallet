namespace Expense_Tracker.Infrastructure.Email;

public sealed class EmailSettings
{
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = "Expnese Tracker";
    public string Password { get; set; } = default!;

    public string Host { get; set; } = default!;
    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string FromName => DisplayName;
    public string FromAddress => Username;
}
