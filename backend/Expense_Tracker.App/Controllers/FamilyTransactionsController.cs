using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Filters;
using Expense_Tracker.Application.Features.DeleteTransaction;
using Expense_Tracker.Application.Features.Transactions.Commands.CreateTransaction;
using Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;
using Expense_Tracker.Application.Features.UpdateTransaction;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Contracts.Requests.Transacations;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
[ApiVersion("1.0")]
public class FamilyTransactionsController(
    IMessageBus bus,
    IFamilyContext familyContext,
    IUserContext userContext
) : ControllerBase
{
    /// <summary>
    /// Retrieves paginated transactions for the authenticated user's family with optional filters.
    /// </summary>
    /// <param name="pageSize">Number of items per page (default: 20, max: 50).</param>
    /// <param name="cursor">Cursor for pagination to retrieve the next page of results.</param>
    /// <param name="transactionType">Filter by transaction type (Income or Expense).</param>
    /// <param name="categoryType">Filter by category type (e.g., Groceries, Rent, etc.).</param>
    /// <param name="minAmount">Filter transactions with amount greater than or equal to this value.</param>
    /// <param name="maxAmount">Filter transactions with amount less than or equal to this value.</param>
    /// <param name="creatorId">Filter transactions created by a specific user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="CursorPagedResponse{T}"/> containing a paginated list of <see cref="TransactionItem"/>.</returns>
    /// <response code="200">Transactions retrieved successfully.</response>
    /// <response code="400">Invalid request parameters (e.g., page size exceeds maximum, invalid filters).</response>
    /// <response code="401">User is not authenticated or family context is missing.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CursorPagedResponse<TransactionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets paginated family transactions with filters.")]
    [EndpointDescription(
        "Returns a cursor-paginated list of transactions for the authenticated user's family, with optional filters for type, category, amount range, and creator. Results are ordered by transaction date descending."
    )]
    [EndpointName("GetFamilyTransactions")]
    [RequireFamily]
    public async Task<ActionResult<CursorPagedResponse<TransactionItem>>> GetTransactions(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        [FromQuery] TransactionType? transactionType = null,
        [FromQuery] CategoryType? categoryType = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] Guid? creatorId = null,
        CancellationToken cancellationToken = default
    )
    {
        // Validate amount range
        if (minAmount.HasValue && maxAmount.HasValue && minAmount.Value > maxAmount.Value)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Invalid Amount Range",
                    Detail = "Minimum amount cannot be greater than maximum amount.",
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }

        Guid familyId = familyContext.FamilyId!.Value;

        var query = new GetFamilyTransactionsQuery(
            FamilyId: familyId,
            PageSize: pageSize,
            Cursor: cursor,
            TransactionType: transactionType,
            CategoryType: categoryType,
            MinAmount: minAmount,
            MaxAmount: maxAmount,
            CreatorId: creatorId
        );

        ErrorOr<CursorPagedResponse<TransactionItem>> result = await bus.InvokeAsync<
            ErrorOr<CursorPagedResponse<TransactionItem>>
        >(query, cancellationToken);

        return result.ToActionResult(this);
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
    [EndpointDescription(
        "Creates a new income or expense transaction for the current family and updates budget tracking accordingly."
    )]
    [EndpointName("CreateTransaction")]
    [RequireFamily]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken
    )
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

        ErrorOr<TransactionResponse> result = await bus.InvokeAsync<ErrorOr<TransactionResponse>>(
            command,
            cancellationToken
        );

        return result.ToActionResult(this);
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
    [EndpointDescription(
        "Updates a transaction by reversing the original budget impact and applying the new values. Only transactions belonging to the user's current family can be updated."
    )]
    [EndpointName("UpdateTransaction")]
    [RequireFamily]
    public async Task<ActionResult<TransactionResponse>> UpdateTransaction(
        [FromRoute] Guid transactionId,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken
    )
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

        ErrorOr<TransactionResponse> result = await bus.InvokeAsync<ErrorOr<TransactionResponse>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
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
    [EndpointDescription(
        "Permanently deletes a transaction and reverses its budget impact. Only transactions belonging to the user's current family can be deleted."
    )]
    [EndpointName("DeleteTransaction")]
    [RequireFamily]
    public async Task<IActionResult> DeleteTransaction(
        [FromRoute] Guid transactionId,
        CancellationToken cancellationToken
    )
    {
        Guid familyId = familyContext.FamilyId!.Value;

        var command = new DeleteTransactionCommand(
            TransactionId: transactionId,
            UserId: userContext.UserId!.Value,
            FamilyId: familyId
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );

        if (result.IsError)
            return result.ToActionResult(this);

        return NoContent();
    }
}
