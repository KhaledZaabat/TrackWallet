using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.UpdateTransaction;

public sealed class UpdateTransactionCommandHandler(
    IRepository<Transaction> transactionRepo,
    IRepository<FamilyUser> familyUserRepo,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
)
{
    public async Task<ErrorOr<TransactionResponse>> Handle(
        UpdateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load transaction with required relations
        var transaction = await transactionRepo.QueryTracked()
            .Include(t => t.Family)
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Transaction));

        // 2. Verify family access
        if (transaction.FamilyId != request.FamilyId)
            return DomainErrors.GeneralErrors.Forbidden("You don't have access to this transaction.");

        bool isFamilyMember = await familyUserRepo.Query()
            .AnyAsync(fu =>
                fu.FamilyId == request.FamilyId &&
                fu.UserId == request.UserId,
                cancellationToken);

        if (!isFamilyMember)
            return DomainErrors.GeneralErrors.Forbidden("You are not a member of this family.");

        // 3. Store old values for budget reversal
        decimal oldAmount = transaction.Amount;
        bool oldIsExpense = transaction.Type == TransactionType.Expense;

        // 4. Update transaction fields
        if (request.Title is not null)
        {
            var result = transaction.Rename(request.Title);
            if (result.IsError)
                return result.Errors;
        }

        if (request.Notes is not null)
        {
            var result = transaction.ChangeNotes(request.Notes);
            if (result.IsError)
                return result.Errors;
        }

        if (request.CategoryId is not null)
        {
            var result = transaction.MoveToCategory(request.CategoryId.Value);
            if (result.IsError)
                return result.Errors;
        }

        if (request.TransactedOn is not null)
        {
            var result = transaction.Reschedule(request.TransactedOn.Value);
            if (result.IsError)
                return result.Errors;
        }

        // 5. Handle amount/type change (budget impact)
        bool amountChanged = request.Amount.HasValue && request.Amount.Value != oldAmount;
        bool typeChanged = request.Type.HasValue && request.Type.Value != transaction.Type;

        if (amountChanged || typeChanged)
        {
            // Reverse old transaction
            var reverseResult = transaction.Family!
                .ReverseTransaction(oldAmount, oldIsExpense);

            if (reverseResult.IsError)
                return reverseResult.Errors;

            // Apply new amount
            if (request.Amount.HasValue)
            {
                var result = transaction.ChangeAmount(request.Amount.Value);
                if (result.IsError)
                    return result.Errors;
            }

            // Apply new type
            if (request.Type.HasValue)
            {
                var result = request.Type.Value == TransactionType.Income
                    ? transaction.MarkAsIncome()
                    : transaction.MarkAsExpense();

                if (result.IsError)
                    return result.Errors;
            }

            // Apply updated transaction to budget
            bool newIsExpense = transaction.Type == TransactionType.Expense;

            var applyResult = transaction.Family!
                .ApplyTransaction(transaction.Amount, newIsExpense);

            if (applyResult.IsError)
                return applyResult.Errors;
        }

        // 6. Save changes
        await transactionRepo.SaveChangesAsync(cancellationToken);

        // 7. Re-query and return full response (same as CreateTransaction)
        var transactionResponse = await transactionRepo.Query()
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
            return DomainErrors.GeneralErrors.NotFound(nameof(Transaction));

        return transactionResponse;
    }
}
