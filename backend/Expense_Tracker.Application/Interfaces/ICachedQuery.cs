using MediatR;

namespace Expense_Tracker.Application.Interfaces;

public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan Expiration { get; }
}
public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;