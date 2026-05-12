using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.UpdatePassword;
using Expense_Tracker.Application.Features.Userr.GetProfile;
using Expense_Tracker.Application.Features.Userr.UpdateProfile;
using Expense_Tracker.Contracts.Requests.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;
[ApiController]
[Route("api/users")]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public sealed class UserController(IMessageBus bus) : ControllerBase
{
    [HttpPost("update-password")]
    [EndpointName("UpdatePassword")]
    [EndpointSummary("Update the authenticated user's password")]
    [EndpointDescription(
        "Allows an authenticated user to change their password by providing the current password."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] UpdatePasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new UpdatePasswordCommand(
                request.CurrentPassword,
                request.NewPassword,
                UserIpAddress: userIpAddress
            ),
            cancellationToken
        );

        return result.ToActionResult(this);
    }
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets user profile.")]
    [EndpointDescription(
        "Returns the authenticated user's complete profile information including name, email, avatar, and notification preferences."
    )]
    [EndpointName("GetUserProfile")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
    {
        var query = new GetProfileQuery();
        ErrorOr<UserProfileResponse> result = await bus.InvokeAsync<ErrorOr<UserProfileResponse>>(
            query,
            ct
        );
        return result.ToActionResult(this);
    }
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates user profile.")]
    [EndpointDescription(
        "Updates the user's profile information including name, bio, and optional avatar image. Accepts multipart/form-data for file uploads."
    )]
    [EndpointName("UpdateUserProfile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileCommand command,
        CancellationToken ct
    )
    {
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
    }
}
