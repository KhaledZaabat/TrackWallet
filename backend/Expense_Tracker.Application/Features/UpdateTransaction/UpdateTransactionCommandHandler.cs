using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.UpdateTransaction;

public sealed class UpdateTransactionCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateTransactionCommand, Result<TransactionResponse>>
{
    public async Task<Result<TransactionResponse>> Handle(
        UpdateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get transaction with family
        Transaction? transaction = await db.Transactions
            .Include(t => t.Family)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure<TransactionResponse>(
                DomainError.NotFound(nameof(Transaction)));

        // 2. Verify user has access
        if (transaction.FamilyId != request.FamilyId)
            return Result.Failure<TransactionResponse>(
                DomainError.Forbidden("You don't have access to this transaction."));

        bool isFamilyMember = await db.FamilyUsers
            .AnyAsync(fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserId,
                cancellationToken);

        if (!isFamilyMember)
            return Result.Failure<TransactionResponse>(
                DomainError.Forbidden("You are not a member of this family."));

        // 3. Store old values for budget reversal
        decimal oldAmount = transaction.Amount;
        bool oldIsExpense = transaction.Type == TransactionType.Expense;

        // 4. Update transaction fields
        if (request.Title is not null)
        {
            Result renameResult = transaction.Rename(request.Title);
            if (renameResult.IsFailure)
                return Result.Failure<TransactionResponse>(renameResult.TryGetError());
        }

        if (request.Notes is not null)
        {
            Result notesResult = transaction.ChangeNotes(request.Notes);
            if (notesResult.IsFailure)
                return Result.Failure<TransactionResponse>(notesResult.TryGetError());
        }

        if (request.CategoryId is not null)
        {
            Result categoryResult = transaction.MoveToCategory(request.CategoryId.Value);
            if (categoryResult.IsFailure)
                return Result.Failure<TransactionResponse>(categoryResult.TryGetError());
        }

        if (request.TransactedOn is not null)
        {
            Result dateResult = transaction.Reschedule(request.TransactedOn.Value);
            if (dateResult.IsFailure)
                return Result.Failure<TransactionResponse>(dateResult.TryGetError());
        }

        // 5. Handle amount or type changes (affects budget)
        bool amountChanged = request.Amount.HasValue && request.Amount.Value != oldAmount;
        bool typeChanged = request.Type.HasValue && request.Type.Value != transaction.Type;

        if (amountChanged || typeChanged)
        {
            // Reverse old transaction
            Result reverseResult = transaction.Family!.ReverseTransaction(oldAmount, oldIsExpense);
            if (reverseResult.IsFailure)
                return Result.Failure<TransactionResponse>(reverseResult.TryGetError());

            // Update amount if changed
            if (request.Amount.HasValue)
            {
                Result amountResult = transaction.ChangeAmount(request.Amount.Value);
                if (amountResult.IsFailure)
                    return Result.Failure<TransactionResponse>(amountResult.TryGetError());
            }

            // Update type if changed
            if (request.Type.HasValue)
            {
                Result typeResult = request.Type.Value == TransactionType.Income
                    ? transaction.MarkAsIncome()
                    : transaction.MarkAsExpense();

                if (typeResult.IsFailure)
                    return Result.Failure<TransactionResponse>(typeResult.TryGetError());
            }

            // Apply new transaction
            bool newIsExpense = transaction.Type == TransactionType.Expense;
            Result applyResult = transaction.Family.ApplyTransaction(transaction.Amount, newIsExpense);
            if (applyResult.IsFailure)
                return Result.Failure<TransactionResponse>(applyResult.TryGetError());
        }

        // 6. Save changes
        await db.SaveChangesAsync(cancellationToken);

        // 7. Return updated transaction
        TransactionResponse response = transaction.Adapt<TransactionResponse>();
        return Result.Success(response);
    }
}