using Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;
using Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.Dashboard;

public sealed record GetDashboardQuery(
    int BudgetHistoryMonths = 1,
    int RecentTransactionsPageSize = 10
) : IRequest<Result<DashboardResponse>>;

public sealed class GetDashboardQueryHandler(
    IAppDbContext db,
    IUserContext userContext,
    IFamilyContext myFamilyContext,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
    ISender sender
) : IRequestHandler<GetDashboardQuery, Result<DashboardResponse>>
{
    public async Task<Result<DashboardResponse>> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Get current user and family context
        Guid? userId = userContext.UserId;
        Guid? familyId = myFamilyContext.FamilyId;

        if (userId is null)
            return Result.Failure<DashboardResponse>(UserError.NotFound());

        if (familyId is null)
            return Result.Failure<DashboardResponse>(
                DomainError.InvalidState(nameof(Family), "No family selected. Please select a family first."));

        // 2. Get family context details
        FamilyContextDto? familyContext = await db.FamilyUsers
            .AsNoTracking()
            .Where(fu => fu.UserId == userId && fu.FamilyId == familyId)
            .Select(fu => new FamilyContextDto(
                fu.FamilyId,
                fu.Family.Name,
                fu.IsParent,
                fu.Family.CurrentBudget
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (familyContext is null)
            return Result.Failure<DashboardResponse>(
                DomainError.NotFound(nameof(Family)));

        // 3. Get user profile information
        var userProfile = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Email,
                u.FullName,
                ProfileImageUrl = u.ProfileImageFileId.HasValue
                    ? fileUrlBuilder.GetUrl(u.ProfileImageFileId.Value)
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile is null)
            return Result.Failure<DashboardResponse>(UserError.NotFound());

        // 4. Get budget history for the family
        Result<List<BudgetHistoryItem>> budgetHistoryResult =
            await sender.Send(
                new GetFamilyBudgetHistoryQuery(familyId.Value, Months: request.BudgetHistoryMonths),
                cancellationToken);

        if (budgetHistoryResult.IsFailure)
            return Result.Failure<DashboardResponse>(budgetHistoryResult.TryGetError());

        List<BudgetHistoryItem> budgetHistory = budgetHistoryResult.TryGetValue();

        // 5. Get recent transactions
        Result<CursorPagedResponse<TransactionItem>> transactionsResult =
            await sender.Send(
                new GetFamilyTransactionsQuery(
                    familyId.Value,
                    PageSize: request.RecentTransactionsPageSize,
                    Cursor: null),
                cancellationToken);

        if (transactionsResult.IsFailure)
            return Result.Failure<DashboardResponse>(transactionsResult.TryGetError());

        CursorPagedResponse<TransactionItem> transactionsPage = transactionsResult.TryGetValue();

        // 6. Build dashboard response
        DashboardResponse response = new(
            UserId: userId.Value.ToString(),
            Email: userProfile.Email,
            FullName: userProfile.FullName,
            FamilyContext: familyContext,
            BudgetHistory: budgetHistory,
            RecentTransactions: transactionsPage.Items.ToList(),
            TransactionsCursor: transactionsPage.NextCursor,
            ProfileImageUrl: userProfile.ProfileImageUrl
        );

        return Result.Success(response);
    }
}

public sealed record DashboardResponse(
    string UserId,
    string Email,
    string FullName,
    FamilyContextDto FamilyContext,
    List<BudgetHistoryItem> BudgetHistory,
    List<TransactionItem> RecentTransactions,
    string? TransactionsCursor,
    string? ProfileImageUrl = null
);