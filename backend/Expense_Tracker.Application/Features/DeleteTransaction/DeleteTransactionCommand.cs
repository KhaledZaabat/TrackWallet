using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.DeleteTransaction;

public sealed record DeleteTransactionCommand(
    Guid TransactionId,
    Guid UserId,
    Guid FamilyId
) : IRequest<Result>;
