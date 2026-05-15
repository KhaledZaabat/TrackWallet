namespace Expense_Tracker.Application.Features.CheckUsername;

public sealed record CheckUsernameQuery(string UserName);

public sealed record UsernameAvailabilityResponse(bool IsAvailable);
