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
    /// Get paginated transactions for a family
    /// </summary>
    /// <param name="pageSize">Number of items per page (max 50)</param>
    /// <param name="cursor">Cursor for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transactions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CursorPagedResponse<TransactionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequireFamily]
    public async Task<ActionResult<CursorPagedResponse<TransactionItem>>> GetTransactions(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {

        Guid FamilyId = familyContext.FamilyId!.Value;


        var query = new GetFamilyTransactionsQuery(
            FamilyId: FamilyId,
            PageSize: pageSize,
            Cursor: cursor
        );

        Result<CursorPagedResponse<TransactionItem>> result =
            await sender.Send(query, cancellationToken);

        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Create a new transaction for the current family
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequireFamily]
    [Authorize]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {

        Guid familyId = familyContext.FamilyId!.Value;


        // Create command
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


    [HttpPut("{transactionId:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpDelete("{transactionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
