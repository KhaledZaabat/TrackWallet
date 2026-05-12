using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.TransactionFolder;
using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandHandler(
    IRepository<Transaction> transactionRepo,
    IRepository<global::Expense_Tracker.Domain.FamilyFolder.Family> familyRepo,
    IRepository<FamilyUser> familyUserRepo,
    IRepository<Category> categoryRepo,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
    IMessageBus bus
)
{
    public async Task<ErrorOr<TransactionResponse>> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var isMember = await familyUserRepo.Query()
            .AnyAsync(fu =>
                fu.UserId == request.UserId &&
                fu.FamilyId == request.FamilyId,
                cancellationToken);

        if (!isMember)
            return DomainErrors.GeneralErrors.NotFound("User is not a member of this family.");

        var categoryExists = await categoryRepo.Query()
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            return DomainErrors.GeneralErrors.NotFound(nameof(Category));

        var transactionResult = Transaction.Create(
            type: request.Type,
            amount: request.Amount,
            transactedOn: request.TransactedOn,
            title: request.Title,
            notes: request.Notes ?? string.Empty,
            createdByID: request.UserId,
            familyID: request.FamilyId,
            categoryID: request.CategoryId
        );

        if (transactionResult.IsError)
            return transactionResult.Errors;

        Transaction transaction = transactionResult.Value;

        var family = await familyRepo.QueryTracked()
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Family));

        bool isExpense = request.Type == Domain.TransactionFolder.Enums.TransactionType.Expense;
        var budgetResult = family.ApplyTransaction(request.Amount, isExpense);

        if (budgetResult.IsError)
            return budgetResult.Errors;

        await transactionRepo.AddAsync(transaction);
        await transactionRepo.SaveChangesAsync(cancellationToken);

        await bus.PublishAsync(new TransactionCreatedEvent(transaction));

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
