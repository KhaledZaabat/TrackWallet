using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.Notifications.ListNotifications;
using Expense_Tracker.Application.Features.Notifications.MarkAllAsRead;
using Expense_Tracker.Application.Features.Notifications.MarkAsRead;
using Expense_Tracker.Application.Features.Notifications.UnreadCount;
using Expense_Tracker.Contracts.Reponses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/notifications")]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public sealed class NotificationsController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Page through the caller's notifications newest-first.
    /// </summary>
    [HttpGet]
    [EndpointName("ListNotifications")]
    [EndpointSummary("List the caller's notifications.")]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NotificationListResponse>> List(
        [FromQuery] bool onlyUnread = false,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        var query = new ListNotificationsQuery(onlyUnread, skip, take);
        ErrorOr<NotificationListResponse> result =
            await bus.InvokeAsync<ErrorOr<NotificationListResponse>>(query, ct);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Returns just the unread count. Used by the SPA to drive the bell-icon
    /// badge and is cheap enough to poll on a short interval.
    /// </summary>
    [HttpGet("unread-count")]
    [EndpointName("GetUnreadNotificationCount")]
    [EndpointSummary("Returns the number of unread notifications.")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UnreadCountResponse>> UnreadCount(CancellationToken ct)
    {
        ErrorOr<UnreadCountResponse> result =
            await bus.InvokeAsync<ErrorOr<UnreadCountResponse>>(new UnreadCountQuery(), ct);
        return result.ToActionResult(this);
    }

    /// <summary>Mark a single notification as read. Idempotent.</summary>
    [HttpPost("{notificationId:guid}/read")]
    [EndpointName("MarkNotificationAsRead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        [FromRoute] Guid notificationId,
        CancellationToken ct)
    {
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new MarkNotificationAsReadCommand(notificationId),
            ct);
        return result.ToActionResult(this);
    }

    /// <summary>Mark every unread notification as read.</summary>
    [HttpPost("read-all")]
    [EndpointName("MarkAllNotificationsAsRead")]
    [ProducesResponseType(typeof(MarkAllAsReadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MarkAllAsReadResponse>> MarkAllAsRead(CancellationToken ct)
    {
        ErrorOr<MarkAllAsReadResponse> result =
            await bus.InvokeAsync<ErrorOr<MarkAllAsReadResponse>>(
                new MarkAllNotificationsAsReadCommand(),
                ct);
        return result.ToActionResult(this);
    }
}
