using Asp.Versioning;
using Expense_Tracker.App.Filters;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.Invitations.Accept;
using Expense_Tracker.Application.Features.Invitations.Decline;
using Expense_Tracker.Application.Features.Invitations.Queries;
using Expense_Tracker.Application.Features.Invitations.Send;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Inv;
using Expense_Tracker.Contracts.Requests.Inv;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
[ApiVersion("1.0")]
public class InvitationsController(ISender sender, IFamilyContext familyContext, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Send a family invitation to a user by email
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequireFamily]
    public async Task<ActionResult<InvitationResponse>> SendInvitation(
        [FromBody] SendInvitationRequest request,
        CancellationToken cancellationToken)
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var command = new SendInvitationCommand(
            InviteeEmail: request.Email,
            IsParent: request.IsParent,
            InviterUserId: userContext.UserId!.Value,
            FamilyId: familyId
        );

        Result<InvitationResponse> result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Accept a family invitation
    /// </summary>
    [HttpPost("{invitationId}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequireFamily]
    public async Task<IActionResult> AcceptInvitation(
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(
            InvitationId: invitationId,
            UserId: userContext.UserId!.Value
        );
        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Decline a family invitation
    /// </summary>
    [HttpPost("{invitationId}/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequireFamily]

    public async Task<IActionResult> DeclineInvitation(
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken)
    {
        var command = new DeclineInvitationCommand(
            InvitationId: invitationId,
            UserId: userContext.UserId!.Value
        );
        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Get all received invitations for the current user (pending only)
    /// </summary>
    [HttpGet("received")]
    [ProducesResponseType(typeof(List<InvitationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequireFamily]

    public async Task<ActionResult<List<InvitationResponse>>> GetReceivedInvitations(
        CancellationToken cancellationToken)
    {
        var query = new GetReceivedInvitationsQuery(userContext.UserId!.Value);
        Result<List<InvitationResponse>> result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Get all sent invitations for the current user's family
    /// </summary>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(List<InvitationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequireFamily]
    public async Task<ActionResult<List<InvitationResponse>>> GetSentInvitations(
        CancellationToken cancellationToken)
    {
        var query = new GetSentInvitationsQuery(
            FamilyId: familyContext.FamilyId!.Value,
            UserId: userContext.UserId!.Value);
        Result<List<InvitationResponse>> result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(HttpContext);
    }




}
