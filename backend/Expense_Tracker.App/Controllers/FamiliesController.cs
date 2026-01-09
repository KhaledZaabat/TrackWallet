using Asp.Versioning;
using Expense_Tracker.App.Filters;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.Family.Commands.CreateFamily;
using Expense_Tracker.Application.Features.Family.Commands.DeleteFamily;
using Expense_Tracker.Application.Features.Family.Commands.KickFamilyMember;
using Expense_Tracker.Application.Features.Family.Commands.LeaveFamily;
using Expense_Tracker.Application.Features.Family.Commands.SelectFamily;
using Expense_Tracker.Application.Features.Family.Commands.UpdateFamily;
using Expense_Tracker.Application.Features.Family.Queries.GetMyFamiliesWithUsers;
using Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;
using Expense_Tracker.Application.Features.GetFamilyUsers;
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
[Authorize]
public class FamiliesController(ISender sender, IUserContext userContext, IFamilyContext familyContext) : ControllerBase
{
    /// <summary>
    /// Retrieves all families associated with the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="FamilyResponse"/> representing user's families.</returns>
    /// <response code="200">Families retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<FamilyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets all user families.")]
    [EndpointDescription("Returns a list of all families that the authenticated user is a member of.")]
    [EndpointName("GetUserFamilies")]

    public async Task<ActionResult<List<FamilyResponse>>> GetUserFamilies(CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User Is not Authorized");

        var query = new GetUserFamiliesQuery(userContext.UserId.Value);
        Result<List<FamilyResponse>> result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Selects a family and retrieves complete family context with dashboard data.
    /// </summary>
    /// <param name="request">Family selection request containing the family ID and device ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="SelectFamilyResponse"/> containing full family context, dashboard data, budget history, recent transactions, and refreshed auth tokens.</returns>
    /// <response code="200">Family selected successfully; returns complete context, dashboard data, and new JWT/refresh tokens.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Specified family not found or user is not a member.</response>
    [HttpPost("select")]
    [ProducesResponseType(typeof(SelectFamilyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Selects a family and loads full context.")]
    [EndpointDescription("Sets the active family for the user session and returns comprehensive family context including user information, family details, budget history, recent transactions, and refreshed authentication tokens (JWT and refresh token).")]
    [EndpointName("SelectFamily")]
    public async Task<ActionResult<SelectFamilyResponse>> SelectFamily(
        [FromBody] SelectFamilyRequest request,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User Is not Authorized");

        var command = new SelectFamilyCommand(
            UserId: userContext.UserId.Value,
            FamilyId: request.FamilyId,
            DeviceId: request.DeviceId
        );

        Result<SelectFamilyResponse> result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Creates a new family with the authenticated user as the parent.
    /// </summary>
    /// <param name="request">Family creation request containing name, initial budget, and bio.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="CreateFamilyResponse"/> containing the newly created family details.</returns>
    /// <response code="201">Family created successfully.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateFamilyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Creates a new family.")]
    [EndpointDescription("Establishes a new family unit with the authenticated user automatically assigned as the parent role. Initializes the family with a name, budget, and optional bio.")]
    [EndpointName("CreateFamily")]
    public async Task<ActionResult<CreateFamilyResponse>> CreateFamily(
        [FromBody] CreateFamilyRequest request,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new CreateFamilyCommand(
            UserId: userContext.UserId.Value,
            Name: request.Name,
            InitialBudget: request.InitialBudget,
            FamilyBio: request.FamilyBio
        );

        Result<CreateFamilyResponse> result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Retrieves the currently selected family with all its members.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active family with member profiles.</returns>
    /// <response code="200">Family retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Family not found or user is not a member.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(FamilyWithMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets current family.")]
    [EndpointDescription("Returns the currently selected family (from family context) including all members and their profile information.")]
    [EndpointName("GetMyFamilyWithUsers")]
    public async Task<ActionResult<FamilyWithMembersResponse>> GetMyFamilyWithUsers(
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var query = new GetMyFamilyWithUsersQuery();

        Result<FamilyWithMembersResponse> result =
            await sender.Send(query, cancellationToken);

        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Retrieves users of the currently selected family (id + name only).
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<FamilyUserSimpleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets family users.")]
    [EndpointDescription("Returns a list of users belonging to the currently selected family. Only user id and full name are returned.")]
    [RequireFamily]

    public async Task<ActionResult<List<FamilyUserSimpleResponse>>> GetFamilyUsers(
        CancellationToken cancellationToken)
    {

        var query = new GetFamilyUsersQuery();

        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Updates the current family's information (name and/or bio).
    /// </summary>
    /// <param name="request">Update request containing new name and/or bio.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="200">Family updated successfully.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not a parent or no family selected.</response>
    /// <response code="404">Family not found.</response>
    [HttpPut]
    [RequireParentRole]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates family information.")]
    [EndpointDescription("Updates the currently selected family's name and/or bio. Only parents can perform this action.")]
    [EndpointName("UpdateFamily")]
    public async Task<IActionResult> UpdateFamily(
        [FromBody] UpdateFamilyRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFamilyCommand(
            FamilyId: familyContext.FamilyId!.Value,
            Name: request.Name,
            FamilyBio: request.FamilyBio
        );

        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Leaves the current family.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="200">Successfully left the family.</response>
    /// <response code="400">Cannot leave - last parent with other members remaining.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">No family selected.</response>
    /// <response code="404">User is not a member of this family.</response>
    [HttpDelete("leave")]
    [RequireFamily]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Leaves the current family.")]
    [EndpointDescription("Removes the authenticated user from the currently selected family. The last parent cannot leave if other members exist. User's transactions remain with the family for historical data.")]
    [EndpointName("LeaveFamily")]
    public async Task<IActionResult> LeaveFamily(CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new LeaveFamilyCommand(
            UserId: userContext.UserId.Value,
            FamilyId: familyContext.FamilyId!.Value
        );

        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Kicks a member from the current family.
    /// </summary>
    /// <param name="userId">The ID of the user to kick.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="200">Member kicked successfully.</response>
    /// <response code="400">Cannot kick yourself or another parent.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not a parent or no family selected.</response>
    /// <response code="404">Target user is not a member of this family.</response>
    [HttpDelete("members/{userId:guid}")]
    [RequireFamily]
    [RequireParentRole]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Kicks a member from the family.")]
    [EndpointDescription("Removes a non-parent member from the currently selected family. Only parents can perform this action. Cannot kick other parents or yourself. The kicked user's transactions remain with the family.")]
    [EndpointName("KickFamilyMember")]
    public async Task<IActionResult> KickFamilyMember(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new KickFamilyMemberCommand(
            FamilyId: familyContext.FamilyId!.Value,
            UserIdToKick: userId,
            RequestingUserId: userContext.UserId.Value
        );

        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Deletes a family by ID.
    /// </summary>
    /// <param name="familyId">The ID of the family to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="200">Family deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not a parent of this family.</response>
    /// <response code="404">Family not found or user is not a member.</response>
    [HttpDelete("{familyId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a family by ID.")]
    [EndpointDescription("Deletes the specified family. Only parents of the family can perform this action. All members are removed, pending invitations are cancelled, but transactions are preserved for historical data.")]
    [EndpointName("DeleteFamily")]
    public async Task<IActionResult> DeleteFamily(
        [FromRoute] Guid familyId,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new DeleteFamilyCommand(
            FamilyId: familyId,
            RequestingUserId: userContext.UserId.Value
        );

        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }
}