using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.UpdateTransaction;

public sealed class UpdateTransactionCommandHandler(
    IAppDbContext db,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
) : IRequestHandler<UpdateTransactionCommand, Result<TransactionResponse>>
{
    public async Task<Result<TransactionResponse>> Handle(
        UpdateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load transaction with required relations
        var transaction = await db.Transactions
            .Include(t => t.Family)
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure<TransactionResponse>(
                DomainError.NotFound(nameof(Transaction)));

        // 2. Verify family access
        if (transaction.FamilyId != request.FamilyId)
            return Result.Failure<TransactionResponse>(
                DomainError.Forbidden("You don't have access to this transaction."));

        bool isFamilyMember = await db.FamilyUsers
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.UserId,
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
            var result = transaction.Rename(request.Title);
            if (result.IsFailure)
                return Result.Failure<TransactionResponse>(result.TryGetError());
        }

        if (request.Notes is not null)
        {
            var result = transaction.ChangeNotes(request.Notes);
            if (result.IsFailure)
                return Result.Failure<TransactionResponse>(result.TryGetError());
        }

        if (request.CategoryId is not null)
        {
            var result = transaction.MoveToCategory(request.CategoryId.Value);
            if (result.IsFailure)
                return Result.Failure<TransactionResponse>(result.TryGetError());
        }

        if (request.TransactedOn is not null)
        {
            var result = transaction.Reschedule(request.TransactedOn.Value);
            if (result.IsFailure)
                return Result.Failure<TransactionResponse>(result.TryGetError());
        }

        // 5. Handle amount/type change (budget impact)
        bool amountChanged = request.Amount.HasValue && request.Amount.Value != oldAmount;
        bool typeChanged = request.Type.HasValue && request.Type.Value != transaction.Type;

        if (amountChanged || typeChanged)
        {
            // Reverse old transaction
            var reverseResult = transaction.Family!
                .ReverseTransaction(oldAmount, oldIsExpense);

            if (reverseResult.IsFailure)
                return Result.Failure<TransactionResponse>(reverseResult.TryGetError());

            // Apply new amount
            if (request.Amount.HasValue)
            {
                var result = transaction.ChangeAmount(request.Amount.Value);
                if (result.IsFailure)
                    return Result.Failure<TransactionResponse>(result.TryGetError());
            }

            // Apply new type
            if (request.Type.HasValue)
            {
                var result = request.Type.Value == TransactionType.Income
                    ? transaction.MarkAsIncome()
                    : transaction.MarkAsExpense();

                if (result.IsFailure)
                    return Result.Failure<TransactionResponse>(result.TryGetError());
            }

            // Apply updated transaction to budget
            bool newIsExpense = transaction.Type == TransactionType.Expense;

            var applyResult = transaction.Family!
                .ApplyTransaction(transaction.Amount, newIsExpense);

            if (applyResult.IsFailure)
                return Result.Failure<TransactionResponse>(applyResult.TryGetError());
        }

        // 6. Save changes
        await db.SaveChangesAsync(cancellationToken);

        // 7. Re-query and return full response (same as CreateTransaction)
        var transactionResponse = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Id == transaction.Id)
            .Select(t => new TransactionResponse(
                TransactionId: t.Id,
                Title: t.Title,
                Amount: t.Amount,
                Type: t.Type,
                TransactedOn: t.TransactedOn,
                Notes: t.Notes,
                CreatedAtUtc: t.CreatedAtUtc,
                Category: new CategoryResponse(
                    CategoryId: t.Category!.Id,
                    Name: t.Category.Type
                ),
                Creator: new CreatorResponse(
                    UserId: t.CreatedBy!.Id,
                    FullName: t.CreatedBy.FullName,
                    ProfileImageUrl: t.CreatedBy.ProfileImageFileId.HasValue
                        ? fileUrlBuilder.GetUrl(t.CreatedBy.ProfileImageFileId.Value)
                        : null
                )
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (transactionResponse is null)
            return Result.Failure<TransactionResponse>(
                DomainError.NotFound(nameof(Transaction)));

        return Result.Success(transactionResponse);
    }
}
