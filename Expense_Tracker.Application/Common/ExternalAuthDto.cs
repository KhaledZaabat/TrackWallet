namespace Expense_Tracker.Application.Common;

public sealed record ExternalAuthDto(
    Guid IdentityId,
    string Email,
    string? FirstName,
    string? LastName,
    string? UserName,
    string? Provider,
    string? PhoneNumber
//string? PictureUrl
);