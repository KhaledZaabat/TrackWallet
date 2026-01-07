using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Family;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.GetFamilyUsers;

public sealed class GetFamilyUsersQueryHandler(IAppDbContext db, IFamilyContext familyContext)
: IRequestHandler<GetFamilyUsersQuery, Result<List<FamilyUserSimpleResponse>>>
{
    public async Task<Result<List<FamilyUserSimpleResponse>>> Handle(
        GetFamilyUsersQuery request,
        CancellationToken cancellationToken)
    {
        bool familyExists = await db.Families
            .AnyAsync(f => f.Id == familyContext.FamilyId, cancellationToken);

        if (!familyExists)
            return Result.Failure<List<FamilyUserSimpleResponse>>(
                DomainError.NotFound("Family"));

        var users = await db.FamilyUsers
            .AsNoTracking()
            .Where(fu => fu.FamilyId == familyContext.FamilyId)
            .Select(fu => new FamilyUserSimpleResponse(
                UserId: fu.User.Id,
                FullName: fu.User.FullName
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(users);
    }
}