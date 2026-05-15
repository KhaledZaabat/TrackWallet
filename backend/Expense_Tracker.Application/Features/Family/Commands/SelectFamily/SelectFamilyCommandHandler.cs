using ErrorOr;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.PushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

/// <summary>
/// Switches the caller's active family. Side-effects: subscribes the user's
/// devices to the family FCM topic and issues new auth cookies scoped to the
/// selected family. The response carries only the new family context — the
/// SPA loads transactions, budget, and members from their own REST endpoints.
/// </summary>
public sealed class SelectFamilyCommandHandler(
    IRepository<FamilyUser> familyUsers,
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IRepository<UserDevice> userDevices,
    IFcmTopicService topicService)
{
    public async Task<ErrorOr<SelectFamilyCommandResult>> Handle(
        SelectFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify user exists.
        ErrorOr<AuthenticatedUser> userResult =
            await identityService.GetUserByIdAsync(request.UserId);

        if (userResult.IsError)
            return userResult.Errors;

        AuthenticatedUser authenticatedUser = userResult.Value;

        // 2. Verify the user is a member and load the family context.
        FamilyContextDto? familyContext = await familyUsers.Query()
            .Where(fu => fu.UserId == request.UserId && fu.FamilyId == request.FamilyId)
            .Select(fu => new FamilyContextDto(
                fu.FamilyId,
                fu.Family.Name,
                fu.IsParent,
                fu.Family.CurrentBudget))
            .FirstOrDefaultAsync(cancellationToken);

        if (familyContext is null)
            return DomainErrors.GeneralErrors.NotFound("Family");

        // 3. Subscribe the user's devices to the family FCM topic so push
        //    notifications scoped to the family arrive while it is selected.
        List<UserDevice> devices = await userDevices.QueryTracked()
            .Where(d => d.UserId == request.UserId && d.IsActive)
            .ToListAsync(cancellationToken);

        if (devices.Count > 0)
        {
            var tokens = devices.Select(d => d.DeviceToken).ToList();
            string familyTopic = Topics.getFamilyTopic(familyContext.FamilyId);

            await topicService.SubscribeToTopicAsync(tokens, familyTopic, cancellationToken);
            foreach (var device in devices)
                device.SubscribeToTopic(familyTopic);
        }

        // 4. Issue new auth tokens carrying the family context. The controller
        //    writes them into HttpOnly cookies — they never appear in the body.
        ErrorOr<AuthDto> tokenResult = await tokenProvider.GenerateJwtTokenWithFamilyAsync(
            authenticatedUser,
            request.DeviceId,
            familyContext,
            cancellationToken);

        if (tokenResult.IsError)
            return tokenResult.Errors;

        AuthDto authDto = tokenResult.Value;

        return new SelectFamilyCommandResult(
            UserId: authDto.UserId,
            Email: authDto.Email,
            FullName: authDto.FullName,
            JwtToken: authDto.JwtToken,
            RefreshToken: authDto.RefreshToken,
            FamilyContext: familyContext);
    }
}
