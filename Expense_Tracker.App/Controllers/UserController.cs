using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Features.UpdatePassword;
using Expense_Tracker.Application.Features.UsersFeatures.Queries.GetUsers;
using Expense_Tracker.Contracts.Requests.Users;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Users.Abstraction;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

/// <summary>
/// Handles user-related operations such as profile actions and administration queries.
/// </summary>
[ApiController]
[Route("api/users")]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public sealed class UserController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <remarks>
    /// The user must be authenticated and provide:
    /// - the current password
    /// - a new password that meets security requirements
    ///
    /// This endpoint is intended for **self-service password updates**.
    /// </remarks>
    /// <param name="request">Contains the current and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Validation error or weak password.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="404">User not found.</response>
    /// <response code="409">New password is the same as the current one.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost("update-password")]
    [EndpointName("UpdatePassword")]
    [EndpointSummary("Update the authenticated user's password")]
    [EndpointDescription("Allows an authenticated user to change their password by providing the current password.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] UpdatePasswordRequest request,
        CancellationToken cancellationToken)
    {
        string userIpAddress = HttpContext.GetClientIp();

        Result result = await sender.Send(
            new UpdatePasswordCommand(
                request.CurrentPassword,
                request.NewPassword,
                UserIpAddress: userIpAddress
            ),
            cancellationToken
        );

        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Retrieves all non-deleted users in the system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This endpoint is restricted to <b>administrators only</b>.
    /// </para>
    /// <para>
    /// The result includes users that are:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Not soft-deleted (<c>IsDeleted = false</c>)</description>
    ///   </item>
    ///   <item>
    ///     <description>Either active or inactive</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// Intended use cases:
    /// <list type="bullet">
    ///   <item><description>Administrative dashboards</description></item>
    ///   <item><description>User management and moderation</description></item>
    ///   <item><description>Audit and compliance views</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="role">Get Users based on role .</param>

    /// <response code="200">Users retrieved successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have administrator privileges.</response>
    [HttpGet]
    [Authorize(Roles = RoleGroups.Admins)]
    [EndpointName("GetAllUsers")]
    [EndpointSummary("Get all users")]
    [EndpointDescription("Returns all non-deleted users, including both active and inactive accounts.")]
    [ProducesResponseType(typeof(IReadOnlyList<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetAll([FromQuery] Role? role,
        CancellationToken ct)
    {
        Result<IReadOnlyList<UserListItemDto>> result =
            await sender.Send(new GetUsersQuery(role), ct);

        return result.ToActionResult(HttpContext);
    }

}
