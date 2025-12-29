using System.Security.Claims;

namespace Expense_Tracker.App.Helpers;

public static class UserExtensions
{
    public static string? GetIdentityId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);



    public static string GetClientIp(this HttpContext context)
    {
        var headers = context.Request.Headers;

        // 1. Check Cloudflare header first (most reliable)
        if (headers.TryGetValue("CF-Connecting-IP", out var cfIp))
            if (IsValidIp(cfIp)) return cfIp!;

        // 2. Check True-Client-IP (Akamai / proxies)
        if (headers.TryGetValue("True-Client-IP", out var trueIp))
            if (IsValidIp(trueIp)) return trueIp!;

        // 3. X-Forwarded-For (IIS ARR / NGINX / Cloudflare fallback)
        if (headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',')[0].Trim();
            if (IsValidIp(ip))
                return ip;
        }

        // 4. Fallback to RemoteIpAddress
        var remote = context.Connection.RemoteIpAddress?.ToString();
        if (IsValidIp(remote))
            return remote!;

        return "Unknown";
    }

    private static bool IsValidIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;
        return System.Net.IPAddress.TryParse(ip, out _);
    }
}