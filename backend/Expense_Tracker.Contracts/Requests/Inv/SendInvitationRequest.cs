using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Contracts.Requests.Inv;

public sealed record SendInvitationRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }

    [Required]
    public required bool IsParent { get; init; }
}
