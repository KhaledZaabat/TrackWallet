using Asp.Versioning;
using Expense_Tracker.App.Filters;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.Dashboard;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiVersion("1.0")]

public class DashboardController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Gets the dashboard data for the currently selected family.
    /// Includes user info, family context, budget history, and recent transactions.
    /// </summary>
    /// <param name="budgetHistoryMonths">Number of months of budget history to retrieve (default: 1)</param>
    /// <param name="recentTransactionsPageSize">Number of recent transactions to retrieve (default: 10)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequireFamily]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(
        [FromQuery] int budgetHistoryMonths = 1,
        [FromQuery] int recentTransactionsPageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetDashboardQuery(
            BudgetHistoryMonths: budgetHistoryMonths,
            RecentTransactionsPageSize: recentTransactionsPageSize
        );

        Result<DashboardResponse> result = await sender.Send(query, ct);

        return result.ToActionResult(HttpContext);
    }
}