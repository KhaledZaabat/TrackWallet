using Asp.Versioning;
using Expense_Tracker.App.Filters;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.DeleteTransaction;
using Expense_Tracker.Application.Features.Transactions.Commands.CreateTransaction;
using Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;
using Expense_Tracker.Application.Features.UpdateTransaction;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Contracts.Requests.Transacations;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
[ApiVersion("1.0")]
public class FamilyTransactionsController(ISender sender, IFamilyContext familyContext, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Retrieves paginated transactions for the authenticated user's family.
    /// </summary>
    /// <param name="pageSize">Number of items per page (default: 20, max: 50).</param>
    /// <param name="cursor">Cursor for pagination to retrieve the next page of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="CursorPagedResponse{T}"/> containing a paginated list of <see cref="TransactionItem"/>.</returns>
    /// <response code="200">Transactions retrieved successfully.</response>
    /// <response code="400">Invalid request parameters (e.g., page size exceeds maximum).</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CursorPagedResponse<TransactionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets paginated family transactions.")]
    [EndpointDescription("Returns a cursor-paginated list of transactions for the authenticated user's family, ordered by transaction date descending.")]
    [EndpointName("GetFamilyTransactions")]
    [RequireFamily]
    public async Task<ActionResult<CursorPagedResponse<TransactionItem>>> GetTransactions(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var query = new GetFamilyTransactionsQuery(
            FamilyId: familyId,
            PageSize: pageSize,
            Cursor: cursor
        );

        Result<CursorPagedResponse<TransactionItem>> result =
            await sender.Send(query, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Creates a new transaction for the authenticated user's family.
    /// </summary>
    /// <param name="request">Transaction creation request containing type, amount, date, title, notes, and category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing the created transaction details.</returns>
    /// <response code="201">Transaction created successfully.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    /// <response code="404">Category not found or does not belong to the family.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new transaction.")]
    [EndpointDescription("Creates a new income or expense transaction for the current family and updates budget tracking accordingly.")]
    [EndpointName("CreateTransaction")]
    [RequireFamily]
    [Authorize]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var command = new CreateTransactionCommand(
            UserId: userContext.UserId!.Value,
            FamilyId: familyId,
            Type: request.Type,
            Amount: request.Amount,
            TransactedOn: request.TransactedOn,
            Title: request.Title,
            Notes: request.Notes,
            CategoryId: request.CategoryId
        );

        Result<TransactionResponse> result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Updates an existing transaction for the authenticated user's family.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction to update.</param>
    /// <param name="request">Transaction update request containing type, amount, date, title, notes, and category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="TransactionResponse"/> containing the updated transaction details.</returns>
    /// <response code="200">Transaction updated successfully.</response>
    /// <response code="400">Invalid request or validation failure.</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    /// <response code="404">Transaction or category not found.</response>
    /// <remarks>
    /// Updates reverse the original transaction's budget impact and apply the new transaction values.
    /// This ensures budget history remains accurate after modifications.
    /// </remarks>
    [HttpPut("{transactionId:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates an existing transaction.")]
    [EndpointDescription("Updates a transaction by reversing the original budget impact and applying the new values. Only transactions belonging to the user's current family can be updated.")]
    [EndpointName("UpdateTransaction")]
    [RequireFamily]
    [Authorize]
    public async Task<ActionResult<TransactionResponse>> UpdateTransaction(
        [FromRoute] Guid transactionId,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var command = new UpdateTransactionCommand(
            TransactionId: transactionId,
            UserId: userContext.UserId!.Value,
            FamilyId: familyId,
            Type: request.Type,
            Amount: request.Amount,
            TransactedOn: request.TransactedOn,
            Title: request.Title,
            Notes: request.Notes,
            CategoryId: request.CategoryId
        );

        Result<TransactionResponse> result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Deletes an existing transaction from the authenticated user's family.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Transaction deleted successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    /// <response code="404">Transaction not found or does not belong to the family.</response>
    /// <remarks>
    /// Deletion reverses the transaction's budget impact to maintain accurate budget tracking.
    /// The transaction is permanently removed and cannot be recovered.
    /// </remarks>
    [HttpDelete("{transactionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a transaction.")]
    [EndpointDescription("Permanently deletes a transaction and reverses its budget impact. Only transactions belonging to the user's current family can be deleted.")]
    [EndpointName("DeleteTransaction")]
    [RequireFamily]
    [Authorize]
    public async Task<IActionResult> DeleteTransaction(
        [FromRoute] Guid transactionId,
        CancellationToken cancellationToken)
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var command = new DeleteTransactionCommand(
            TransactionId: transactionId,
            UserId: userContext.UserId!.Value,
            FamilyId: familyId
        );

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult(HttpContext);

        return NoContent();
    }
}