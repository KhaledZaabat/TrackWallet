namespace Expense_Tracker.Application.Features.Refresh.Dto;

public record RefreshTokenDto(
    string Token,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    bool IsRevoked
);