namespace Expense_Tracker.Contracts.Requests.Family;

/// <summary>
/// Body for <c>POST /api/families/{familyId}/select</c>. The family id lives
/// in the URL; this carries only the device identifier needed for FCM topic
/// subscription.
/// </summary>
public sealed record SelectFamilyRequest(string DeviceId);
