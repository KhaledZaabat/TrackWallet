using Family = Expense_Tracker.Domain.FamilyFolder.Family;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.FamilyFolder;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Family.Commands.UpdateFamily;

public sealed class UpdateFamilyCommandHandler(
    IRepository<global::Expense_Tracker.Domain.FamilyFolder.Family> families
)
{
    public async Task<ErrorOr<Success>> Handle(
        UpdateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        var family = await families.QueryTracked()
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId && !f.IsDeleted, cancellationToken);

        if (family is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Family));

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameResult = family.UpdateName(request.Name);
            if (nameResult.IsError)
                return nameResult.Errors;
        }

        // Update bio if provided (can be set to null/empty to clear it)
        if (request.FamilyBio != null)
        {
            var bioResult = family.UpdateBio(request.FamilyBio);
            if (bioResult.IsError)
                return bioResult.Errors;
        }

        await families.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
