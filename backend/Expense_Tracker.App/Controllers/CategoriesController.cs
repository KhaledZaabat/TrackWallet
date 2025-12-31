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
    /// Get all available categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequireFamily]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var query = new GetCategoriesQuery();
        Result<List<CategoryResponse>> result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }
}