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

        var family = await db.FamilyUsers
            .AsNoTracking()
            .Where(fu =>
                fu.FamilyId == familyId &&
                fu.UserId == userId &&
                !fu.Family.IsDeleted)
            .Select(fu => fu.Family)
            .Select(f => new FamilyWithMembersResponse(
                Id: f.Id,
                Name: f.Name,
                CurrentBudget: f.CurrentBudget,
                FamilyBio: f.FamilyBio,
                Members: f.FamilyUsers
                    .Where(mu => !mu.User.IsDeleted)
                    .Select(mu => new FamilyUserProfileResponse(
                        UserId: mu.User.Id,
                        FullName: mu.User.FullName,
                        UserName: mu.User.UserName,
                        BirthDate: mu.User.BirthDate,
                        IsMale: mu.User.IsMale,
                        ProfileImageUrl: mu.User.ProfileImageFileId.HasValue
                            ? fileUrlBuilder.GetUrl(mu.User.ProfileImageFileId.Value)
                            : null,
                        IsParent: mu.IsParent
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(ct);

        if (family is null)
            return Result.Failure<FamilyWithMembersResponse>(
                DomainError.NotFound(nameof(Family)));

        return Result.Success(family);
    }
}