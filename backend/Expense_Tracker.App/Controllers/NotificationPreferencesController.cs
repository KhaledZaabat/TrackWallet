using Asp.Versioning;
using Expense_Tracker.App.Filters;
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
    /// Update notification preferences
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequireFamily]
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