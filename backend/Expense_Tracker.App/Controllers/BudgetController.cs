using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Filters;
using Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/budget")]
[Authorize]
[ApiVersion("1.0")]
public sealed class BudgetController(IMessageBus bus, IFamilyContext familyContext) : ControllerBase
{
    /// <summary>
    /// Retrieves budget history for the authenticated user's selected family.
    /// </summary>
    /// <param name="months">Number of months of history to retrieve (1-24). Defaults to 1.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="BudgetHistoryItem"/> containing monthly budget snapshots.</returns>
    /// <response code="200">Budget history retrieved successfully.</response>
    /// <response code="400">Invalid parameters (e.g., months out of range).</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<BudgetHistoryItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Retrieves family budget history.")]
    [EndpointDescription(
        "Returns monthly budget data including planned amounts, actual spending, and variance for each category. Family context is extracted from JWT token."
    )]
    [EndpointName("GetBudgetHistory")]
    [RequireFamily]
    public async Task<ActionResult<List<BudgetHistoryItem>>> GetBudgetHistory(
        [FromQuery] int months = 1,
        CancellationToken cancellationToken = default
    )
    {
        Guid familyId = familyContext.FamilyId!.Value;
        var query = new GetFamilyBudgetHistoryQuery(FamilyId: familyId, Months: months);
        ErrorOr<List<BudgetHistoryItem>> result = await bus.InvokeAsync<
            ErrorOr<List<BudgetHistoryItem>>
        >(query, cancellationToken);
        return result.ToActionResult(this);
    }
}
