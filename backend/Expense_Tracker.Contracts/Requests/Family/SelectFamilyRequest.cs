namespace Expense_Tracker.Contracts.Requests.Family;

public sealed record SelectFamilyRequest(
    Guid FamilyId,
    string DeviceId
);