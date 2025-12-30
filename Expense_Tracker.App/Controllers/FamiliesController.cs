using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.Family.Commands.CreateFamily;
using Expense_Tracker.Application.Features.Family.Commands.SelectFamily;
using Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Requests.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[Authorize]
[ApiController]
[Route("api/families")]
[ApiVersion("1.0")]

public class FamiliesController(ISender sender, IUserContext userContext
) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FamilyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<FamilyResponse>>> GetUserFamilies(CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User Is not Authorized");
        var query = new GetUserFamiliesQuery(userContext.UserId.Value);

        Result<List<FamilyResponse>> result = await sender.Send(query, cancellationToken);


        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Select a family and get full context including transactions and budget history
    /// </summary>
    /// <param name="request">Family selection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete family context with auth tokens</returns>
    [HttpPost("select")]
    [ProducesResponseType(typeof(SelectFamilyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SelectFamilyResponse>> SelectFamily(
        [FromBody] SelectFamilyRequest request,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User Is not Authorized");

        // Create command
        var command = new SelectFamilyCommand(
            UserId: userContext.UserId.Value,
            FamilyId: request.FamilyId,
            DeviceId: request.DeviceId
        );

        Result<SelectFamilyResponse> result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Create a new family (creator becomes parent automatically)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateFamilyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<ActionResult<CreateFamilyResponse>> CreateFamily(
        [FromBody] CreateFamilyRequest request,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        // Create command
        var command = new CreateFamilyCommand(
            UserId: userContext.UserId.Value,
            Name: request.Name,
            InitialBudget: request.InitialBudget,
            FamilyBio: request.FamilyBio
        );

        // Execute command
        Result<CreateFamilyResponse> result = await sender.Send(command, cancellationToken);

        //  if (result.IsFailure)
        return result.ToActionResult(HttpContext);

        //return CreatedAtAction(
        //    nameof(GetUserFamilies),
        //    new { },
        //    result.TryGetValue()
        //);
    }
}
