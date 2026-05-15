using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Domain.FamilyUserFolder;
using ErrorOr;
using Expense_Tracker.Application.Features.Family.Queries.GetMyFamiliesWithUsers;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;

public sealed class GetMyFamilyWithUsersQueryHandler(
      IRepository<Family> familyRepo,
      IRepository<FamilyUser> familyUserRepo,
      IRepository<User> userRepo,
      IFileUrlResolver fileUrlResolver,
      IFamilyContext familyContext,
      IUserContext userContext
  )
{
    public async Task<ErrorOr<FamilyWithMembersResponse>> Handle(
        GetMyFamilyWithUsersQuery request,
        CancellationToken ct)
    {
        if (familyContext.FamilyId is null || userContext.UserId is null)
            return DomainErrors.UserErrors.Unauthorized();

        Guid familyId = familyContext.FamilyId.Value;
        Guid userId = userContext.UserId.Value;
        var family = await (
                 from f in familyRepo.QueryTracked()
                 where f.Id == familyId && !f.IsDeleted
                 select new FamilyWithMembersResponse(
                     Id: f.Id,
                     Name: f.Name,
                     CurrentBudget: f.CurrentBudget,
                     FamilyBio: f.FamilyBio,
                     Members: (
                         from fu in familyUserRepo.Query()
                         join u in userRepo.Query() on fu.UserId equals u.Id
                         where fu.FamilyId == f.Id && !u.IsDeleted
                         select new FamilyUserProfileResponse(
                             UserId: u.Id,
                             FullName: u.FullName,
                             UserName: u.UserName,
                             BirthDate: u.BirthDate,
                             IsMale: u.IsMale,
                             ProfileImageUrl: u.ProfileImageFileId.HasValue
                                 ? fileUrlResolver.GetUrl(u.ProfileImageFileId.Value)
                                 : null,
                             IsParent: fu.IsParent
                         )
                     ).ToList()
                 )
             ).FirstOrDefaultAsync(ct);

        if (family is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Family));

        return family;
    }
}
