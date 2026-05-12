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
