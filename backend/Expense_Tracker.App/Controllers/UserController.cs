using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.UpdatePassword;
using Expense_Tracker.Application.Features.Userr.GetProfile;
using Expense_Tracker.Application.Features.Userr.UpdateProfile;
using Expense_Tracker.Contracts.Requests.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
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

    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
    {
        var query = new GetProfileQuery();
        Result<UserProfileResponse> result = await sender.Send(query, ct);



        return result.ToActionResult(HttpContext);
    }

    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileCommand command,
        CancellationToken ct)
    {
        Result result = await sender.Send(command, ct);



        return result.ToActionResult(HttpContext);
    }
}






