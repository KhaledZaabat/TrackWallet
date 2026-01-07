using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.FamiliyHistoryBudget.Queries;
using Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

public sealed class SelectFamilyCommandHandler(
    IAppDbContext db,
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
    ISender sender,
    IUserDeviceRepository deviceRepository
) : IRequestHandler<SelectFamilyCommand, Result<SelectFamilyResponse>>
{
    public async Task<Result<SelectFamilyResponse>> Handle(
        SelectFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify user exists
        Result<AuthenticatedUser> userResult =
            await identityService.GetUserByIdAsync(request.UserId);

        if (userResult.IsFailure)
            return Result.Failure<SelectFamilyResponse>(userResult.TryGetError());

        AuthenticatedUser authenticatedUser = userResult.TryGetValue();

        // 2. Verify user is member of the family and get family context
        FamilyContextDto? familyContext = await db.FamilyUsers
            .AsNoTracking()
            .Where(fu => fu.UserId == request.UserId && fu.FamilyId == request.FamilyId)
            .Select(fu => new FamilyContextDto(

                fu.FamilyId,
                fu.Family.Name,
                fu.IsParent,
                fu.Family.CurrentBudget
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (familyContext is null)
            return Result.Failure<SelectFamilyResponse>(
                DomainError.NotFound(nameof(Family)));
        await deviceRepository.SubscribeToTopicAsync(
           request.UserId,
           Topics.getFamilyTopic(familyContext.FamilyId),
           cancellationToken);


        // 3. Generate JWT tokens with family context
        Result<AuthDto> tokenResult =
            await tokenProvider.GenerateJwtTokenWithFamilyAsync(
                authenticatedUser,
                request.DeviceId,
                familyContext,
                cancellationToken);

        if (tokenResult.IsFailure)
            return Result.Failure<SelectFamilyResponse>(tokenResult.TryGetError());

        AuthDto authDto = tokenResult.TryGetValue();

        // 4. Get user's profile image
        Guid? profileFileId = await db.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => u.ProfileImageFileId)
            .FirstOrDefaultAsync(cancellationToken);

        string? profileImageUrl = fileUrlBuilder.GetUrl(profileFileId);

        // 5. Get budget history for the family (last month by default)
        Result<List<BudgetHistoryItem>> budgetHistoryResult =
            await sender.Send(
                new GetFamilyBudgetHistoryQuery(request.FamilyId, Months: 1),
                cancellationToken);

        if (budgetHistoryResult.IsFailure)
            return Result.Failure<SelectFamilyResponse>(budgetHistoryResult.TryGetError());

        List<BudgetHistoryItem> budgetHistory = budgetHistoryResult.TryGetValue();

        // 6. Get recent transactions (paginated)
        Result<CursorPagedResponse<TransactionItem>> transactionsResult =
            await sender.Send(
                new GetFamilyTransactionsQuery(request.FamilyId, PageSize: 10, Cursor: null),
                cancellationToken);

        if (transactionsResult.IsFailure)
            return Result.Failure<SelectFamilyResponse>(transactionsResult.TryGetError());

        CursorPagedResponse<TransactionItem> transactionsPage = transactionsResult.TryGetValue();



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

        return Result.Success(response);
    }
}
