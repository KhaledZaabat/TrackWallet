using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.Categories.Queries.GetCategories;
using Expense_Tracker.Contracts.Reponses.Category;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/categories")]
[ApiVersion("1.0")]
public class CategoriesController(IMessageBus bus) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets all  categories for transactions .")]
    [EndpointDescription(
        "Returns a complete list of budget categories available for the authenticated user's family."
    )]
    [EndpointName("GetCategories")]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories(
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesQuery();
        ErrorOr<List<CategoryResponse>> result = await bus.InvokeAsync<
            ErrorOr<List<CategoryResponse>>
        >(query, cancellationToken);
        return result.ToActionResult(this);
    }
}
