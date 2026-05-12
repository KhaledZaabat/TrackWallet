using Expense_Tracker.Domain.Users;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;
using Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.PushNotifications;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

public sealed class SelectFamilyCommandHandler(
    IRepository<FamilyUser> familyUsers,
    IRepository<User> users,
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
    IMessageBus bus,
    IRepository<UserDevice> userDevices,
    IFcmTopicService topicService
)
{
    public async Task<ErrorOr<SelectFamilyResponse>> Handle(
        SelectFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify user exists
        ErrorOr<AuthenticatedUser> userResult =
            await identityService.GetUserByIdAsync(request.UserId);

        if (userResult.IsError)
            return userResult.Errors;

        AuthenticatedUser authenticatedUser = userResult.Value;

        // 2. Verify user is member of the family and get family context
        FamilyContextDto? familyContext = await familyUsers.Query()
            .Where(fu => fu.UserId == request.UserId && fu.FamilyId == request.FamilyId)
            .Select(fu => new FamilyContextDto(
                fu.FamilyId,
                fu.Family.Name,
                fu.IsParent,
                fu.Family.CurrentBudget
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (familyContext is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Family));

        // Subscribe devices to family topic
        List<UserDevice> devices = await userDevices.QueryTracked()
            .Where(x => x.UserId == request.UserId && x.IsActive)
            .ToListAsync(cancellationToken);

        if (devices.Count > 0)
        {
            var tokens = devices.Select(d => d.DeviceToken).ToList();
            string familyTopic = Topics.getFamilyTopic(familyContext.FamilyId);
            await topicService.SubscribeToTopicAsync(tokens, familyTopic, cancellationToken);

            foreach (var device in devices)
            {
                device.SubscribeToTopic(familyTopic);
            }
        }

        // 3. Generate JWT tokens with family context
        ErrorOr<AuthDto> tokenResult =
            await tokenProvider.GenerateJwtTokenWithFamilyAsync(
                authenticatedUser,
                request.DeviceId,
                familyContext,
                cancellationToken);

        if (tokenResult.IsError)
            return tokenResult.Errors;

        AuthDto authDto = tokenResult.Value;

        // 4. Get user's profile image
        Guid? profileFileId = await users.Query()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.ProfileImageFileId)
            .FirstOrDefaultAsync(cancellationToken);

        string? profileImageUrl = fileUrlBuilder.GetUrl(profileFileId);

        // 5. Get budget history for the family (last month by default)
        ErrorOr<List<BudgetHistoryItem>> budgetHistoryResult =
            await bus.InvokeAsync<ErrorOr<List<BudgetHistoryItem>>>(
                new GetFamilyBudgetHistoryQuery(request.FamilyId, Months: 1),
                cancellationToken);

        if (budgetHistoryResult.IsError)
            return budgetHistoryResult.Errors;

        List<BudgetHistoryItem> budgetHistory = budgetHistoryResult.Value;

        // 6. Get recent transactions (paginated)
        ErrorOr<CursorPagedResponse<TransactionItem>> transactionsResult =
            await bus.InvokeAsync<ErrorOr<CursorPagedResponse<TransactionItem>>>(
                new GetFamilyTransactionsQuery(request.FamilyId, PageSize: 10, Cursor: null),
                cancellationToken);

        if (transactionsResult.IsError)
            return transactionsResult.Errors;

        CursorPagedResponse<TransactionItem> transactionsPage = transactionsResult.Value;

        // 8. Build response
        SelectFamilyResponse response = new(
            UserId: authDto.UserId,
            Email: authDto.Email,
            FullName: authDto.FullName,
            JwtToken: authDto.JwtToken,
            RefreshToken: authDto.RefreshToken,
            FamilyContext: familyContext,
            BudgetHistory: budgetHistory,
            RecentTransactions: transactionsPage.Items.ToList(),
            ProfileImageUrl: profileImageUrl
        );

        return response;
    }
}
