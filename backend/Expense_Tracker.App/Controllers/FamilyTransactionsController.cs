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
