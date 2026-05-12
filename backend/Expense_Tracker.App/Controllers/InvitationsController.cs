using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Filters;
using Expense_Tracker.Application.Features.Invitations.Accept;
using Expense_Tracker.Application.Features.Invitations.Cancel;
using Expense_Tracker.Application.Features.Invitations.Decline;
using Expense_Tracker.Application.Features.Invitations.Queries;
using Expense_Tracker.Application.Features.Invitations.Send;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Contracts.Requests.Inv;
using Expense_Tracker.Domain.Invitation.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
[ApiVersion("1.0")]
public class InvitationsController(
    IMessageBus bus,
    IFamilyContext familyContext,
    IUserContext userContext
) : ControllerBase
{
    /// <summary>
    /// Sends a family invitation to a user by email.
    /// </summary>
    /// <param name="request">Invitation request containing invitee email and parent role flag.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="InvitationResponse"/> containing the created invitation details.</returns>
    /// <response code="201">Invitation sent successfully; FCM notification sent to invitee.</response>
    /// <response code="400">Invalid request or validation failure (e.g., user already in family).</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    /// <response code="403">User does not have permission to send invitations (parents only).</response>
    /// <remarks>
    /// Only parents can send family invitations. Upon successful invitation, an FCM push notification
    /// is sent to the invitee to alert them of the new invitation.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [EndpointSummary("Sends a family invitation.")]
    [EndpointDescription(
        "Sends an invitation to join the family by email. Only parents can send invitations. The invitee receives an FCM notification upon successful invitation creation."
    )]
    [EndpointName("SendInvitation")]
    [RequireParentRole]
    public async Task<ActionResult<InvitationResponse>> SendInvitation(
        [FromBody] SendInvitationRequest request,
        CancellationToken cancellationToken
    )
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var command = new SendInvitationCommand(
            InviteeEmail: request.Email,
            IsParent: request.IsParent,
            InviterUserId: userContext.UserId!.Value,
            FamilyId: familyId
        );

        ErrorOr<InvitationResponse> result = await bus.InvokeAsync<ErrorOr<InvitationResponse>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Accepts a family invitation and adds the user to the family.
    /// </summary>
    /// <param name="invitationId">The unique identifier of the invitation to accept.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if invitation accepted.</returns>
    /// <response code="200">Invitation accepted successfully; FCM notification sent to family parents.</response>
    /// <response code="400">Invalid request or invitation already processed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not the invitation recipient.</response>
    /// <response code="404">Invitation not found.</response>
    /// <remarks>
    /// Upon acceptance, the user is added to the family with the designated role (parent or child).
    /// All family parents receive an FCM notification about the new member.
    /// </remarks>
    [HttpPost("{invitationId}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Accepts a family invitation.")]
    [EndpointDescription(
        "Accepts a pending invitation and adds the user to the family. Family parents receive an FCM notification about the new member."
    )]
    [EndpointName("AcceptInvitation")]
    [Authorize]
    public async Task<IActionResult> AcceptInvitation(
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken
    )
    {
        var command = new AcceptInvitationCommand(
            InvitationId: invitationId,
            UserId: userContext.UserId!.Value
        );
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Declines a family invitation.
    /// </summary>
    /// <param name="invitationId">The unique identifier of the invitation to decline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if invitation declined.</returns>
    /// <response code="200">Invitation declined successfully; FCM notification sent to inviter.</response>
    /// <response code="400">Invalid request or invitation already processed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to decline (parents only can decline on behalf of family).</response>
    /// <response code="404">Invitation not found.</response>
    /// <remarks>
    /// Only parents can decline invitations. Upon declining, the invitation status is updated
    /// and the inviter receives an FCM notification about the declined invitation.
    /// </remarks>
    [HttpPost("{invitationId}/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Declines a family invitation.")]
    [EndpointDescription(
        "Declines a pending invitation. Only parents can perform this action. The inviter receives an FCM notification about the declined invitation."
    )]
    [EndpointName("DeclineInvitation")]
    [Authorize]
    public async Task<IActionResult> DeclineInvitation(
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken
    )
    {
        var command = new DeclineInvitationCommand(
            InvitationId: invitationId,
            UserId: userContext.UserId!.Value
        );
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Retrieves all invitations received by the authenticated user.
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Accepted, Declined, Cancelled). Defaults to Pending if not specified.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="InvitationResponse"/> representing invitations.</returns>
    /// <response code="200">Invitations retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("received")]
    [ProducesResponseType(typeof(List<InvitationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets received invitations.")]
    [EndpointDescription(
        "Returns a list of invitations received by the authenticated user, optionally filtered by status."
    )]
    [EndpointName("GetReceivedInvitations")]
    [Authorize]
    public async Task<ActionResult<List<InvitationResponse>>> GetReceivedInvitations(
        [FromQuery] InvitationStatus? status,
        CancellationToken cancellationToken
    )
    {
        var query = new GetReceivedInvitationsQuery(userContext.UserId!.Value, status);
        ErrorOr<List<InvitationResponse>> result = await bus.InvokeAsync<
            ErrorOr<List<InvitationResponse>>
        >(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Retrieves all invitations sent by the authenticated user's family.
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Accepted, Declined, Cancelled).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of <see cref="InvitationResponse"/> representing sent invitations.</returns>
    /// <response code="200">Invitations retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to view sent invitations.</response>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(List<InvitationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [EndpointSummary("Gets sent invitations.")]
    [EndpointDescription(
        "Returns a list of invitations sent from the current family, optionally filtered by status."
    )]
    [EndpointName("GetSentInvitations")]
    [RequireFamily]
    public async Task<ActionResult<List<InvitationResponse>>> GetSentInvitations(
        [FromQuery] InvitationStatus? status,
        CancellationToken cancellationToken
    )
    {
        var query = new GetSentInvitationsQuery(
            FamilyId: familyContext.FamilyId!.Value,
            UserId: userContext.UserId!.Value,
            Status: status
        );

        ErrorOr<List<InvitationResponse>> result = await bus.InvokeAsync<
            ErrorOr<List<InvitationResponse>>
        >(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Cancels a sent family invitation.
    /// </summary>
    /// <param name="invitationId">The unique identifier of the invitation to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success response if invitation cancelled.</returns>
    /// <response code="200">Invitation cancelled successfully; FCM notification sent to invitee.</response>
    /// <response code="400">Invalid request or invitation already processed.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have permission to cancel (parents only).</response>
    /// <response code="404">Invitation not found.</response>
    /// <remarks>
    /// Only parents can cancel sent invitations. Upon cancellation, the invitation status is updated
    /// and the invitee receives an FCM notification about the cancelled invitation.
    /// </remarks>
    [HttpPost("{invitationId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Cancels a sent invitation.")]
    [EndpointDescription(
        "Cancels a pending invitation that was sent from the family. Only parents can perform this action. The invitee receives an FCM notification about the cancellation."
    )]
    [EndpointName("CancelInvitation")]
    [RequireParentRole]
    public async Task<IActionResult> CancelInvitation(
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken
    )
    {
        var command = new CancelInvitationCommand(
            InvitationId: invitationId,
            RequesterId: userContext.UserId!.Value
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
    }
}
