using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Filters;
using Expense_Tracker.Application.Features.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiVersion("1.0")]
public class DashboardController(IMessageBus bus) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets family dashboard overview.")]
    [EndpointDescription(
        "Returns a comprehensive dashboard with user information, family context, budget history, and recent transactions for the selected family."
    )]
    [EndpointName("GetDashboard")]
    [RequireFamily]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(
        [FromQuery] int budgetHistoryMonths = 1,
        [FromQuery] int recentTransactionsPageSize = 10,
        CancellationToken ct = default
    )
    {
        var query = new GetDashboardQuery(
            BudgetHistoryMonths: budgetHistoryMonths,
            RecentTransactionsPageSize: recentTransactionsPageSize
        );
        ErrorOr<DashboardResponse> result = await bus.InvokeAsync<ErrorOr<DashboardResponse>>(
            query,
            ct
        );
        return result.ToActionResult(this);
    }
}
