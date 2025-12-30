using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Family.Commands.CreateFamily;

public sealed class CreateFamilyCommandHandler(
    IAppDbContext db
) : IRequestHandler<CreateFamilyCommand, Result<CreateFamilyResponse>>
{
    public async Task<Result<CreateFamilyResponse>> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify user exists
        var userExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
            return Result.Failure<CreateFamilyResponse>(
                DomainError.NotFound(nameof(User)));

        // 2. Create family
        Result<Domain.FamilyFolder.Family> familyResult = Domain.FamilyFolder.Family.Create(
            name: request.Name,
            currentBudget: request.InitialBudget,
            createdBy: request.UserId,
            familyBio: request.FamilyBio
        );

        if (familyResult.IsFailure)
            return Result.Failure<CreateFamilyResponse>(familyResult.TryGetError());

        Domain.FamilyFolder.Family family = familyResult.TryGetValue();

        // 3. Add creator as parent member
        Result<FamilyUser> familyUserResult = FamilyUser.Create(
            familyId: family.Id,
            userId: request.UserId,
            isParent: true,
            invitedById: request.UserId
        );

        if (familyUserResult.IsFailure)
            return Result.Failure<CreateFamilyResponse>(familyUserResult.TryGetError());

        FamilyUser familyUser = familyUserResult.TryGetValue();



        // 5. Save all entities
        db.Families.Add(family);
        db.FamilyUsers.Add(familyUser);

        await db.SaveChangesAsync(cancellationToken);

        // 6. Build response
        var response = new CreateFamilyResponse(
            FamilyId: family.Id,
            Name: family.Name,
            CurrentBudget: family.CurrentBudget,
            FamilyBio: family.FamilyBio,
            CreatedAtUtc: family.CreatedAtUtc,
            IsParent: true,
            MemberCount: 1
        );

        return Result.Success(response);
    }
}