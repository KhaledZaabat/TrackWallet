using Expense_Tracker.Application.Features.Family.Queries.GetMyFamiliesWithUsers;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.FamilyFolder;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class GetMyFamilyWithUsersQueryHandler(
      IAppDbContext db,
      [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder,
      IFamilyContext familyContext,
      IUserContext userContext
  ) : IRequestHandler<GetMyFamilyWithUsersQuery, Result<FamilyWithMembersResponse>>
{
    public async Task<Result<FamilyWithMembersResponse>> Handle(
        GetMyFamilyWithUsersQuery request,
        CancellationToken ct)
    {
        if (familyContext.FamilyId is null || userContext.UserId is null)
            return Result.Failure<FamilyWithMembersResponse>(
                UserError.Unauthorized());

        Guid familyId = familyContext.FamilyId.Value;
        Guid userId = userContext.UserId.Value;
        var family = await (
                 from f in db.Families
                 where f.Id == familyId && !f.IsDeleted
                 select new FamilyWithMembersResponse(
                     Id: f.Id,
                     Name: f.Name,
                     CurrentBudget: f.CurrentBudget,
                     FamilyBio: f.FamilyBio,
                     Members: (
                         from fu in db.FamilyUsers
                         join u in db.Users on fu.UserId equals u.Id
                         where fu.FamilyId == f.Id && !u.IsDeleted
                         select new FamilyUserProfileResponse(
                             UserId: u.Id,
                             FullName: u.FullName,
                             UserName: u.UserName,
                             BirthDate: u.BirthDate,
                             IsMale: u.IsMale,
                             ProfileImageUrl: u.ProfileImageFileId.HasValue
                                 ? fileUrlBuilder.GetUrl(u.ProfileImageFileId.Value)
                                 : null,
                             IsParent: fu.IsParent
                         )
                     ).ToList()
                 )
             ).FirstOrDefaultAsync(ct);

        if (family is null)
            return Result.Failure<FamilyWithMembersResponse>(
                DomainError.NotFound(nameof(Family)));

        return Result.Success(family);
    }
}