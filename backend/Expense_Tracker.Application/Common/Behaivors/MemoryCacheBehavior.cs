using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Common.Behaivors;

public class MemoryCacheBehavior<TRequest, TResponse>(
    IMemoryCache cache,
    ILogger<MemoryCacheBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{


    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct);
        }

        if (cache.TryGetValue(cachedRequest.CacheKey, out TResponse? cachedResponse))
        {
            logger.LogInformation("Cache hit for {RequestName}", typeof(TRequest).Name);
            return cachedResponse!;
        }

        logger.LogInformation("Cache miss for {RequestName}", typeof(TRequest).Name);

        TResponse response = await next(ct);

        if (response is Result res && res.IsSuccess)
        {
            cache.Set(
                        cachedRequest.CacheKey,
                        response,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = cachedRequest.Expiration
                        });

            logger.LogInformation("Cached result for {RequestName}", typeof(TRequest).Name);
        }

        return response;
    }
}