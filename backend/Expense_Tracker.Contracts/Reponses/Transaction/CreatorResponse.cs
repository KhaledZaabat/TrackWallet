namespace Expense_Tracker.Contracts.Reponses.Transaction;

public sealed record CreatorResponse(
    Guid UserId,
    string FullName,
    string? ProfileImageUrl
);
