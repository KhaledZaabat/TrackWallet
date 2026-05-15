using System.Collections.Frozen;
using System.Text.RegularExpressions;
using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Expense_Tracker.Application.Features.CheckUsername;

public sealed partial class CheckUsernameQueryHandler(
    IRepository<User> userRepo,
    IMemoryCache cache
)
{
    [GeneratedRegex(@"^[a-zA-Z0-9._-]{3,50}$", RegexOptions.CultureInvariant)]
    private static partial Regex UsernamePatternGenerated();

    private static readonly Regex UsernamePattern = UsernamePatternGenerated();

    private static readonly FrozenSet<string> ReservedNormalized = new[]
    {
        "ADMIN",
        "ADMINISTRATOR",
        "ROOT",
        "SYSTEM",
        "API",
        "SUPPORT",
        "HELP",
        "INFO",
        "OWNER",
        "MODERATOR",
        "MOD",
        "STAFF",
        "OFFICIAL",
        "TRACKWALLET",
        "TEST",
        "NULL",
        "UNDEFINED",
        "ANONYMOUS",
        "ME",
        "SELF",
        "USER",
        "USERS",
        "ACCOUNT",
        "ACCOUNTS",
        "SETTINGS",
        "LOGIN",
        "LOGOUT",
        "REGISTER",
        "SIGNUP",
        "SIGNIN",
    }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly TimeSpan TakenTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan AvailableTtl = TimeSpan.FromSeconds(15);

    public async Task<ErrorOr<UsernameAvailabilityResponse>> Handle(
        CheckUsernameQuery query,
        CancellationToken ct
    )
    {
        string? input = query.UserName?.Trim();

        // 1. Format gate — cheapest path. No cache touch, no DB.
        if (string.IsNullOrEmpty(input) || !UsernamePattern.IsMatch(input))
            return new UsernameAvailabilityResponse(false);

        string normalized = User.Normalize(input);

        // 2. Reserved gate.
        if (ReservedNormalized.Contains(normalized))
            return new UsernameAvailabilityResponse(false);

        string cacheKey = $"uname:{normalized}";

        // 3. Stampede-safe lookup

        Lazy<Task<bool>> lazy = cache.GetOrCreate(
            cacheKey,
            entry =>
            {
                entry.Size = 1;
                entry.Priority = CacheItemPriority.Low;

                entry.AbsoluteExpirationRelativeToNow = AvailableTtl;
                return new Lazy<Task<bool>>(() =>
                    CheckIsAvailableAsync(normalized, CancellationToken.None)
                );
            }
        )!;

        bool isAvailable = await lazy.Value.WaitAsync(ct).ConfigureAwait(false);

        if (!isAvailable)
        {
            cache.Set(
                cacheKey,
                lazy,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TakenTtl,
                    Size = 1,
                    Priority = CacheItemPriority.Low,
                }
            );
        }

        return new UsernameAvailabilityResponse(isAvailable);
    }

    private async Task<bool> CheckIsAvailableAsync(string normalizedUserName, CancellationToken ct)
    {
        bool taken = await userRepo
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.NormalizedUserName == normalizedUserName, ct)
            .ConfigureAwait(false);

        return !taken;
    }
}
