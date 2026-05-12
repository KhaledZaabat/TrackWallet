using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.DeleteTransaction;

public sealed class DeleteTransactionCommandHandler(
    IRepository<Transaction> transactionRepo,
    IRepository<FamilyUser> familyUserRepo
)
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteTransactionCommand request,
        CancellationToken cancellationToken)
    {
        Transaction? transaction = await transactionRepo.QueryTracked()
            .Include(t => t.Family)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Transaction));

        if (transaction.FamilyId != request.FamilyId)
            return DomainErrors.GeneralErrors.Forbidden("You don't have access to this transaction.");

        bool isFamilyMember = await familyUserRepo.Query()
            .AnyAsync(fu => fu.FamilyId == request.FamilyId && fu.UserId == request.UserId,
                cancellationToken);

        if (!isFamilyMember)
            return DomainErrors.GeneralErrors.Forbidden("You are not a member of this family.");

        bool isExpense = transaction.Type == TransactionType.Expense;
        var reverseResult = transaction.Family!.ReverseTransaction(
            transaction.Amount,
            isExpense);

        if (reverseResult.IsError)
            return reverseResult.Errors;

        transactionRepo.Remove(transaction);
        await transactionRepo.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
