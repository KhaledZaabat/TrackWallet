using Asp.Versioning;
using Expense_Tracker.App.Filters;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.Categories.Queries.GetCategories;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
[ApiVersion("1.0")]
public class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all available transactions categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="CategoryResponse"/> representing all categories.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets all  categories for transactions .")]
    [EndpointDescription("Returns a complete list of budget categories available for the authenticated user's family.")]
    [EndpointName("GetCategories")]
    [RequireFamily]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        Result<List<CategoryResponse>> result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }
}
