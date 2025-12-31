using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.DeleteTransaction;

public sealed class DeleteTransactionCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteTransactionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get transaction with family
        Transaction? transaction = await db.Transactions
            .Include(t => t.Family)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure(
                DomainError.NotFound(nameof(Transaction)));

        // 2. Verify user has access to this family
        if (transaction.FamilyId != request.FamilyId)
            return Result.Failure(
                DomainError.Forbidden("You don't have access to this transaction."));

        // 3. Verify user is member of the family
        bool isFamilyMember = await db.FamilyUsers
            .AnyAsync(fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserId,
                cancellationToken);

        if (!isFamilyMember)
            return Result.Failure(
                DomainError.Forbidden("You are not a member of this family."));

        // 4. Reverse the transaction impact on family budget
        bool isExpense = transaction.Type == TransactionType.Expense;
        Result reverseResult = transaction.Family!.ReverseTransaction(
            transaction.Amount,
            isExpense);

        if (reverseResult.IsFailure)
            return reverseResult;

        // 5. Delete transaction
        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}