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
    [Authorize]
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
    /// Retrieves the authenticated user's profile information.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="UserProfileResponse"/> containing user profile details.</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User profile not found.</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets user profile.")]
    [EndpointDescription("Returns the authenticated user's complete profile information including name, email, avatar, and notification preferences.")]
    [EndpointName("GetUserProfile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
    {
        var query = new GetProfileQuery();
        Result<UserProfileResponse> result = await sender.Send(query, ct);
        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Updates the authenticated user's profile information.
    /// </summary>
    /// <param name="command">Profile update command containing user details and optional avatar image.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success response if profile updated.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Invalid request, validation failure, or unsupported file format.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User profile not found.</response>
    /// <remarks>
    /// Accepts multipart/form-data for profile updates including an optional avatar image.
    /// Supported image formats: JPEG, PNG, GIF. Maximum file size may apply.
    /// </remarks>
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates user profile.")]
    [EndpointDescription("Updates the user's profile information including name, bio, and optional avatar image. Accepts multipart/form-data for file uploads.")]
    [EndpointName("UpdateUserProfile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileCommand command,
        CancellationToken ct)
    {
        Result result = await sender.Send(command, ct);
        return result.ToActionResult(HttpContext);
    }
}






