using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.UpdateNotificationPreferences;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Requests.Notifications;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/notification-preferences")]
[Authorize]
[ApiVersion("1.0")]
public class NotificationPreferencesController(ISender sender, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Updates notification preferences for the authenticated user.
    /// </summary>
    /// <param name="request">Notification preferences containing email and push notification settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on successful update.</returns>
    /// <response code="204">Notification preferences updated successfully.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Updates notification preferences.")]
    [EndpointDescription("Updates the user's email and push notification preferences. Changes take effect immediately.")]
    [EndpointName("UpdateNotificationPreferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateNotificationPreferencesCommand(
            UserId: userContext.UserId!.Value,
            EmailNotifications: request.EmailNotifications,
            PushNotifications: request.PushNotifications
        );
        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }
}