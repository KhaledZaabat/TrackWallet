using Expense_Tracker.Domain.FamilyUserFolder;
using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Queries.GetUserFamilies;

public sealed class GetUserFamiliesQueryHandler(
    IRepository<FamilyUser> familyUserRepo,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
)
{
    public async Task<ErrorOr<List<FamilyResponse>>> Handle(
        GetUserFamiliesQuery request,
        CancellationToken cancellationToken)
    {
        Guid userId = request.userId;

        // Get all families where the user is a member with all related data
        var familiesData = await familyUserRepo.Query()
            .Where(fu => fu.UserId == userId && !fu.Family.IsDeleted)
            .Select(fu => new
            {
                FamilyId = fu.Family.Id,
                FamilyName = fu.Family.Name,
                CurrentBudget = fu.Family.CurrentBudget,
                FamilyBio = fu.Family.FamilyBio,

                Members = familyUserRepo.Query()
                    .Where(member => member.FamilyId == fu.Family.Id && !member.User.IsDeleted)
                    .Select(member => new
                    {
                        UserId = member.UserId,
                        FullName = member.User.FullName,
                        ProfileImageFileId = member.User.ProfileImageFileId,
                        IsParent = member.IsParent
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // Map to response DTOs with profile image URLs
        var familyResponses = familiesData.Select(family =>
        {
            // Map each member to FamilyMemberProfile with profile image URL
            var memberProfiles = family.Members.Select(member =>
            {
                string? profileImageUrl = fileUrlBuilder.GetUrl(member.ProfileImageFileId);
                return new FamilyMemberProfile(
                    UserId: member.UserId,
                    FullName: member.FullName,
                    ProfileImageUrl: profileImageUrl,
                    IsParent: member.IsParent
                );
            }).ToList();

            // Create the family response
            return new FamilyResponse(
                Id: family.FamilyId,
                Name: family.FamilyName,
                CurrentBudget: family.CurrentBudget,
                FamilyBio: family.FamilyBio,
                Members: memberProfiles
            );
        }).ToList();

        // Return successful result with all families
        return familyResponses;
    }
}
