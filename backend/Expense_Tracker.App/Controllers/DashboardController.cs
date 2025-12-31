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
    /// Retrieves the dashboard overview for the currently selected family.
    /// </summary>
    /// <param name="budgetHistoryMonths">Number of months of budget history to include (default: 1).</param>
    /// <param name="recentTransactionsPageSize">Number of recent transactions to include (default: 10).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="DashboardResponse"/> containing user info, family context, budget history, and recent transactions.</returns>
    /// <response code="200">Dashboard data retrieved successfully.</response>
    /// <response code="400">Invalid request parameters.</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    /// <response code="404">Requested family or data not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets family dashboard overview.")]
    [EndpointDescription("Returns a comprehensive dashboard with user information, family context, budget history, and recent transactions for the selected family.")]
    [EndpointName("GetDashboard")]
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
