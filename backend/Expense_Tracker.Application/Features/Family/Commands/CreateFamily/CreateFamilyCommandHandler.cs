using DomainFamily = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Users;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Commands.CreateFamily;

public sealed class CreateFamilyCommandHandler(
    IRepository<User> users,
    IRepository<DomainFamily> families,
    IRepository<FamilyUser> familyUsers
)
{
    public async Task<ErrorOr<CreateFamilyResponse>> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify user exists
        var userExists = await users.Query()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
            return DomainErrors.GeneralErrors.NotFound(nameof(User));

        // 2. Create family
        var familyResult = DomainFamily.Create(
            name: request.Name,
            currentBudget: request.InitialBudget,
            createdBy: request.UserId,
            familyBio: request.FamilyBio
        );

        if (familyResult.IsError)
            return familyResult.Errors;

        DomainFamily family = familyResult.Value;

        // 3. Add creator as parent member
        var familyUserResult = FamilyUser.Create(
            familyId: family.Id,
            userId: request.UserId,
            isParent: true,
            invitedById: request.UserId
        );

        if (familyUserResult.IsError)
            return familyUserResult.Errors;

        FamilyUser familyUser = familyUserResult.Value;

        // 5. Save all entities
        await families.AddAsync(family, cancellationToken);
        await familyUsers.AddAsync(familyUser, cancellationToken);

        await families.SaveChangesAsync(cancellationToken);

        // 6. Build response
        var response = new CreateFamilyResponse(
            Id: family.Id,
            Name: family.Name,
            CurrentBudget: family.CurrentBudget,
            FamilyBio: family.FamilyBio,
            CreatedAtUtc: family.CreatedAtUtc,
            IsParent: true,
            MemberCount: 1
        );

        return response;
    }
}
