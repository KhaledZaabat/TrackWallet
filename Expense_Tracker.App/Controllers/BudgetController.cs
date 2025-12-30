using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/budget")]
[Authorize]
public sealed class BudgetController(ISender sender, IFamilyContext familyContext) : ControllerBase
{
    /// <summary>
    /// Get budget history for the current user's selected family.
    /// The family context is automatically retrieved from the JWT token.
    /// </summary>
    /// <param name="months">Number of months to retrieve (default: 1, max: 24)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of budget history items</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<BudgetHistoryItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BudgetHistoryItem>>> GetBudgetHistory(
        [FromQuery] int months = 1,
        CancellationToken cancellationToken = default)
    {
        Guid FamilyId = familyContext.FamilyId!.Value;
        var query = new GetFamilyBudgetHistoryQuery(
            FamilyId: FamilyId,
            Months: months
        );

        Result<List<BudgetHistoryItem>> result =
            await sender.Send(query, cancellationToken);

        return result.ToActionResult(HttpContext);
    }
}