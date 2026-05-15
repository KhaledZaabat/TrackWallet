namespace Expense_Tracker.Application.Common.Settings;


public sealed class FileUrlOptions
{
    public const string SectionName = "Files";

    /// <summary>
    /// Absolute base URL used to prefix file URLs (e.g.
    /// <c>https://api.trackwallet.example</c>). When unset, the resolver
    /// falls back to the current HTTP request and finally to a relative path.
    /// </summary>
    public string? PublicBaseUrl { get; init; }
}
