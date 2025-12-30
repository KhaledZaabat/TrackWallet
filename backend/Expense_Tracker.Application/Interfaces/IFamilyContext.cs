using Expense_Tracker.Contracts.Reponses.Identity;

namespace Expense_Tracker.Application.Interfaces;

public interface IFamilyContext
{
    /// <summary>
    /// Gets the current family ID from JWT token claims
    /// </summary>
    Guid? FamilyId { get; }

    /// <summary>
    /// Gets the current family name from JWT token claims
    /// </summary>
    string? FamilyName { get; }

    /// <summary>
    /// Gets whether the current user is a parent in the family
    /// </summary>
    bool IsParent { get; }

    /// <summary>
    /// Gets the complete family context from JWT token claims
    /// </summary>
    FamilyContextDto? GetFamilyContext();

    /// <summary>
    /// Checks if user has a valid family context
    /// </summary>
    bool HasFamilyContext { get; }
}