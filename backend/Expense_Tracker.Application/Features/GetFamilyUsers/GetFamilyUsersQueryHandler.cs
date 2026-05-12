using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.GetFamilyUsers;

public sealed class GetFamilyUsersQueryHandler(
    IRepository<global::Expense_Tracker.Domain.FamilyFolder.Family> familyRepo,
    IRepository<FamilyUser> familyUserRepo,
    IFamilyContext familyContext)
{
    public async Task<ErrorOr<List<FamilyUserSimpleResponse>>> Handle(
        GetFamilyUsersQuery request,
        CancellationToken cancellationToken)
    {
        bool familyExists = await familyRepo.QueryTracked()
            .AnyAsync(f => f.Id == familyContext.FamilyId, cancellationToken);

        if (!familyExists)
            return DomainErrors.GeneralErrors.NotFound("Family");

        var users = await familyUserRepo.Query()
            .Where(fu => fu.FamilyId == familyContext.FamilyId)
            .Select(fu => new FamilyUserSimpleResponse(
                UserId: fu.User.Id,
                FullName: fu.User.FullName
            ))
            .ToListAsync(cancellationToken);

        return users;
    }
}
