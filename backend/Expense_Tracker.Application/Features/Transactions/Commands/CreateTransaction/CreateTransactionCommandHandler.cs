using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.TransactionFolder;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandHandler(
    IAppDbContext db,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
) : IRequestHandler<CreateTransactionCommand, Result<TransactionResponse>>
{
    public async Task<Result<TransactionResponse>> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify user is a member of the family
        var isMember = await db.FamilyUsers
            .AsNoTracking()
            .AnyAsync(fu =>
                fu.UserId == request.UserId &&
                fu.FamilyId == request.FamilyId,
                cancellationToken);

        if (!isMember)
            return Result.Failure<TransactionResponse>(
                DomainError.NotFound("User is not a member of this family."));

        // 2. Verify category exists
        var categoryExists = await db.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            return Result.Failure<TransactionResponse>(
                DomainError.NotFound(nameof(Category)));

        // 3. Create transaction
        Result<Transaction> transactionResult = Transaction.Create(
            type: request.Type,
            amount: request.Amount,
            transactedOn: request.TransactedOn,
            title: request.Title,
            notes: request.Notes ?? string.Empty,
            createdByID: request.UserId,
            familyID: request.FamilyId,
            categoryID: request.CategoryId
        );

        if (transactionResult.IsFailure)
            return Result.Failure<TransactionResponse>(transactionResult.TryGetError());

        Transaction transaction = transactionResult.TryGetValue();

        // 4. Update family budget
        var family = await db.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<TransactionResponse>(
                DomainError.NotFound(nameof(Family)));

        bool isExpense = request.Type == Domain.TransactionFolder.Enums.TransactionType.Expense;
        Result budgetResult = family.ApplyTransaction(request.Amount, isExpense);

        if (budgetResult.IsFailure)
            return Result.Failure<TransactionResponse>(budgetResult.TryGetError());

        // 5. Save transaction and updated family
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        // 6. Get complete transaction details for response
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
                    Name: t.Category.Name,
                    Icon: t.Category.IconName
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