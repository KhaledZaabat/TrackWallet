using FluentValidation;

namespace Expense_Tracker.Application.Features.Family.Commands.KickFamilyMember;

public sealed class KickFamilyMemberCommandValidator : AbstractValidator<KickFamilyMemberCommand>
{
    public KickFamilyMemberCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.UserIdToKick)
            .NotEmpty()
            .WithMessage("User ID to kick is required.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user ID is required.");

        RuleFor(x => x)
            .Must(x => x.UserIdToKick != x.RequestingUserId)
            .WithMessage("You cannot kick yourself. Use the leave family endpoint instead.");
    }
}
