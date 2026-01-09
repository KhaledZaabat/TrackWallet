using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.Family.Commands.UpdateFamily;

public sealed class UpdateFamilyCommandHandler(
    IAppDbContext db
) : IRequestHandler<UpdateFamilyCommand, Result>
{
    public async Task<Result> Handle(
        UpdateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find the family
        var family = await db.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId && !f.IsDeleted, cancellationToken);

        if (family is null)
            return Result.Failure(
                DomainError.NotFound(nameof(Domain.FamilyFolder.Family)));

        // 2. Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameResult = family.UpdateName(request.Name);
            if (nameResult.IsFailure)
                return nameResult;
        }

        // 3. Update bio if provided (can be set to null/empty to clear it)
        if (request.FamilyBio != null)
        {
            var bioResult = family.UpdateBio(request.FamilyBio);
            if (bioResult.IsFailure)
                return bioResult;
        }

        // 4. Save changes
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
