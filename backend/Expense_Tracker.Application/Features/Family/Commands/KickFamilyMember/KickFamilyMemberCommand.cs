using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Family.Commands.KickFamilyMember;

public sealed record KickFamilyMemberCommand(
    Guid FamilyId,
    Guid UserIdToKick,
    Guid RequestingUserId
) : IRequest<Result>;
