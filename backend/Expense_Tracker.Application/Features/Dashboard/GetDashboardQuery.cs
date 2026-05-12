using Expense_Tracker.Domain.FamilyUserFolder;
using ErrorOr;
using Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;
using Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;
using Wolverine;

namespace Expense_Tracker.Application.Features.Dashboard;

public sealed record GetDashboardQuery(
    int BudgetHistoryMonths = 1,
    int RecentTransactionsPageSize = 10
);

public sealed class GetDashboardQueryHandler(
    IRepository<FamilyUser> familyUserRepo,
    IRepository<User> userRepo,
    IUserContext userContext,
    IFamilyContext myFamilyContext,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
    IMessageBus bus
)
{
    public async Task<ErrorOr<DashboardResponse>> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Get current user and family context
        Guid? userId = userContext.UserId;
        Guid? familyId = myFamilyContext.FamilyId;

        if (userId is null)
            return DomainErrors.UserErrors.NotFound();

        if (familyId is null)
            return DomainErrors.GeneralErrors.InvalidState(nameof(Family), "No family selected. Please select a family first.");

        // 2. Get family context details
        FamilyContextDto? familyContext = await familyUserRepo.Query()
            .Where(fu => fu.UserId == userId && fu.FamilyId == familyId)
            .Select(fu => new FamilyContextDto(
                fu.FamilyId,
                fu.Family.Name,
                fu.IsParent,
                fu.Family.CurrentBudget
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (familyContext is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Family));

        // 3. Get user profile information
        var userProfile = await userRepo.Query()
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
            return DomainErrors.UserErrors.NotFound();

        // 4. Get budget history for the family
        var budgetHistoryResult = await bus.InvokeAsync<ErrorOr<List<BudgetHistoryItem>>>(
            new GetFamilyBudgetHistoryQuery(familyId.Value, Months: request.BudgetHistoryMonths),
            cancellationToken);

        if (budgetHistoryResult.IsError)
            return budgetHistoryResult.FirstError;

        List<BudgetHistoryItem> budgetHistory = budgetHistoryResult.Value;

        // 5. Get recent transactions
        var transactionsResult = await bus.InvokeAsync<ErrorOr<CursorPagedResponse<TransactionItem>>>(
            new GetFamilyTransactionsQuery(
                familyId.Value,
                PageSize: request.RecentTransactionsPageSize,
                Cursor: null),
            cancellationToken);

        if (transactionsResult.IsError)
            return transactionsResult.FirstError;

        CursorPagedResponse<TransactionItem> transactionsPage = transactionsResult.Value;

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

        return response;
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
