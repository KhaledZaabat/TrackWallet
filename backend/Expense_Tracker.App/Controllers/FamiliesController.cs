using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Auth;
using Expense_Tracker.App.Filters;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/families")]
[ApiVersion("1.0")]
[Authorize]
public class FamiliesController(
    IMessageBus bus,
    IUserContext userContext,
    IFamilyContext familyContext,
    IAuthCookieWriter authCookies
) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FamilyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets all user families.")]
    [EndpointDescription(
        "Returns a list of all families that the authenticated user is a member of."
    )]
    [EndpointName("GetUserFamilies")]
    public async Task<ActionResult<List<FamilyResponse>>> GetUserFamilies(
        CancellationToken cancellationToken
    )
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User Is not Authorized");

        var query = new GetUserFamiliesQuery(userContext.UserId.Value);
        ErrorOr<List<FamilyResponse>> result = await bus.InvokeAsync<ErrorOr<List<FamilyResponse>>>(
            query,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
    [HttpPost("{familyId:guid}/select")]
    [ProducesResponseType(typeof(SelectFamilyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Selects a family as the active context.")]
    [EndpointDescription(
        "Sets the active family for the user session, refreshes auth cookies (JWT + refresh token + CSRF) with the new family context, and subscribes the user's devices to the family's FCM topic. The response carries only the new family context — transactions, budget history, and members are loaded separately from their dedicated endpoints."
    )]
    [EndpointName("SelectFamily")]
    public async Task<ActionResult<SelectFamilyResponse>> SelectFamily(
        [FromRoute] Guid familyId,
        [FromBody] SelectFamilyRequest request,
        CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User Is not Authorized");

        var command = new SelectFamilyCommand(
            UserId: userContext.UserId.Value,
            FamilyId: familyId,
            DeviceId: request.DeviceId);

        ErrorOr<SelectFamilyCommandResult> result =
            await bus.InvokeAsync<ErrorOr<SelectFamilyCommandResult>>(command, cancellationToken);

        if (result.IsError)
            return this.Problem(result.Errors);

        SelectFamilyCommandResult value = result.Value;

        // Tokens never leave the server — they ride in HttpOnly cookies.
        authCookies.WriteAccessCookie(HttpContext, value.JwtToken.Token, value.JwtToken.ExpiresAt);
        authCookies.WriteRefreshCookie(HttpContext, value.RefreshToken.Token, value.RefreshToken.ExpiresAt);
        authCookies.IssueCsrfCookie(HttpContext);

        return Ok(new SelectFamilyResponse(
            UserId: value.UserId,
            Email: value.Email,
            FullName: value.FullName,
            FamilyContext: value.FamilyContext));
    }
    [HttpPost]
    [ProducesResponseType(typeof(CreateFamilyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Creates a new family.")]
    [EndpointDescription(
        "Establishes a new family unit with the authenticated user automatically assigned as the parent role. Initializes the family with a name, budget, and optional bio."
    )]
    [EndpointName("CreateFamily")]
    public async Task<ActionResult<CreateFamilyResponse>> CreateFamily(
        [FromBody] CreateFamilyRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new CreateFamilyCommand(
            UserId: userContext.UserId.Value,
            Name: request.Name,
            InitialBudget: request.InitialBudget,
            FamilyBio: request.FamilyBio
        );

        ErrorOr<CreateFamilyResponse> result = await bus.InvokeAsync<ErrorOr<CreateFamilyResponse>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
    [HttpGet("me")]
    [ProducesResponseType(typeof(FamilyWithMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets current family.")]
    [EndpointDescription(
        "Returns the currently selected family (from family context) including all members and their profile information."
    )]
    [EndpointName("GetMyFamilyWithUsers")]
    public async Task<ActionResult<FamilyWithMembersResponse>> GetMyFamilyWithUsers(
        CancellationToken cancellationToken
    )
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var query = new GetMyFamilyWithUsersQuery();

        ErrorOr<FamilyWithMembersResponse> result = await bus.InvokeAsync<
            ErrorOr<FamilyWithMembersResponse>
        >(query, cancellationToken);

        return result.ToActionResult(this);
    }
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<FamilyUserSimpleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets family users.")]
    [EndpointDescription(
        "Returns a list of users belonging to the currently selected family. Only user id and full name are returned."
    )]
    [RequireFamily]
    public async Task<ActionResult<List<FamilyUserSimpleResponse>>> GetFamilyUsers(
        CancellationToken cancellationToken
    )
    {
        var query = new GetFamilyUsersQuery();

        var result = await bus.InvokeAsync<ErrorOr<List<FamilyUserSimpleResponse>>>(
            query,
            cancellationToken
        );

        return result.ToActionResult(this);
    }
    [HttpPut]
    [RequireParentRole]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates family information.")]
    [EndpointDescription(
        "Updates the currently selected family's name and/or bio. Only parents can perform this action."
    )]
    [EndpointName("UpdateFamily")]
    public async Task<IActionResult> UpdateFamily(
        [FromBody] UpdateFamilyRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateFamilyCommand(
            FamilyId: familyContext.FamilyId!.Value,
            Name: request.Name,
            FamilyBio: request.FamilyBio
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
    [HttpDelete("leave")]
    [RequireFamily]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Leaves the current family.")]
    [EndpointDescription(
        "Removes the authenticated user from the currently selected family. The last parent cannot leave if other members exist. User's transactions remain with the family for historical data."
    )]
    [EndpointName("LeaveFamily")]
    public async Task<IActionResult> LeaveFamily(CancellationToken cancellationToken)
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new LeaveFamilyCommand(
            UserId: userContext.UserId.Value,
            FamilyId: familyContext.FamilyId!.Value
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
    [HttpDelete("members/{userId:guid}")]
    [RequireFamily]
    [RequireParentRole]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Kicks a member from the family.")]
    [EndpointDescription(
        "Removes a non-parent member from the currently selected family. Only parents can perform this action. Cannot kick other parents or yourself. The kicked user's transactions remain with the family."
    )]
    [EndpointName("KickFamilyMember")]
    public async Task<IActionResult> KickFamilyMember(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken
    )
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new KickFamilyMemberCommand(
            FamilyId: familyContext.FamilyId!.Value,
            UserIdToKick: userId,
            RequestingUserId: userContext.UserId.Value
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
    [HttpDelete("{familyId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a family by ID.")]
    [EndpointDescription(
        "Deletes the specified family. Only parents of the family can perform this action. All members are removed, pending invitations are cancelled, but transactions are preserved for historical data."
    )]
    [EndpointName("DeleteFamily")]
    public async Task<IActionResult> DeleteFamily(
        [FromRoute] Guid familyId,
        CancellationToken cancellationToken
    )
    {
        if (!userContext.UserId.HasValue)
            return Unauthorized("User is not authorized");

        var command = new DeleteFamilyCommand(
            FamilyId: familyId,
            RequestingUserId: userContext.UserId.Value
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
}
