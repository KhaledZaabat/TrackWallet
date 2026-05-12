using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.UpdateNotificationPreferences;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Requests.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/notification-preferences")]
[Authorize]
[ApiVersion("1.0")]
public class NotificationPreferencesController(IMessageBus bus, IUserContext userContext)
    : ControllerBase
{
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Updates notification preferences.")]
    [EndpointDescription(
        "Updates the user's email and push notification preferences. Changes take effect immediately."
    )]
    [EndpointName("UpdateNotificationPreferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateNotificationPreferencesCommand(
            UserId: userContext.UserId!.Value,
            EmailNotifications: request.EmailNotifications,
            PushNotifications: request.PushNotifications
        );
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
}
