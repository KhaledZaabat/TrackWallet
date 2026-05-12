using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.App.Auth;

public sealed class SilentRefreshMiddleware
{
    /// <summary>Per-request re-entry guard.</summary>
    public const string MarkerKey = "__trackwallet_silent_refresh_ran";

    /// <summary>Set by the logout action to prevent re-issuing cookies on the logout response.</summary>
    public const string LogoutSkipKey = "AuthLogoutInProgress";

    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<JwtSettings> _jwtOpts;
    private readonly IOptionsMonitor<AuthCookieOptions> _cookieOpts;
    private readonly IMemoryCache _graceCache;
    private readonly ILogger<SilentRefreshMiddleware> _log;

    public SilentRefreshMiddleware(
        RequestDelegate next,
        IOptionsMonitor<JwtSettings> jwtOpts,
        IOptionsMonitor<AuthCookieOptions> cookieOpts,
        IMemoryCache graceCache,
        ILogger<SilentRefreshMiddleware> log
    )
    {
        _next = next;
        _jwtOpts = jwtOpts;
        _cookieOpts = cookieOpts;
        _graceCache = graceCache;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.Items.ContainsKey(MarkerKey) || ctx.Items.ContainsKey(LogoutSkipKey))
        {
            await _next(ctx);
            return;
        }

        if (!EndpointAuthInspector.RequiresAuthorization(ctx))
        {
            await _next(ctx);
            return;
        }

        JwtSettings jwt = _jwtOpts.CurrentValue;
        AuthCookieOptions cookies = _cookieOpts.CurrentValue;

        string? currentAccess = ctx.Request.Cookies[cookies.AccessCookieName];
        string? rawRefresh = ctx.Request.Cookies[cookies.RefreshCookieName];

        if (!ShouldRotate(currentAccess, jwt))
        {
            await _next(ctx);
            return;
        }

        ctx.Items[MarkerKey] = true;

        IAuthCookieWriter writer = ctx.RequestServices.GetRequiredService<IAuthCookieWriter>();

        if (string.IsNullOrEmpty(rawRefresh))
        {
            writer.ClearAuthCookies(ctx);
            await _next(ctx);
            return;
        }

        string? newAccessToken = await TryRotateAsync(ctx, rawRefresh, writer, jwt);

        if (newAccessToken is null)
        {
            // Rotation failed — cookies already cleared. Let the pipeline produce 401.
            await _next(ctx);
            return;
        }

        // Inject the fresh token into the Authorization header so JwtBearerHandler
        // picks it up via OnMessageReceived on its first (and only) call this request.
        ctx.Request.Headers.Authorization = $"Bearer {newAccessToken}";

        await _next(ctx);
    }

    /// <summary>
    /// Returns <see langword="true"/> when the access cookie is missing, unreadable,
    /// expired, or within the silent-refresh threshold window.
    /// Reading <c>exp</c> from the unvalidated cookie is intentional — a tampered
    /// expiry can only cause unnecessary rotation (past) or a 401 from
    /// <c>UseAuthentication</c> (future with bad signature). Neither is a privilege
    /// escalation.
    /// </summary>
    private static bool ShouldRotate(string? accessCookie, JwtSettings jwt)
    {
        if (string.IsNullOrEmpty(accessCookie))
            return true;

        if (!TryReadExpiry(accessCookie, out DateTimeOffset exp))
            return true;

        TimeSpan remaining =
            exp - DateTimeOffset.UtcNow - TimeSpan.FromSeconds(jwt.ClockSkewSeconds);
        return remaining <= jwt.SilentRefreshThresholdAsTimeSpan;
    }

    private static bool TryReadExpiry(string rawJwt, out DateTimeOffset expiry)
    {
        expiry = default;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(rawJwt))
                return false;

            JwtSecurityToken parsed = handler.ReadJwtToken(rawJwt);
            expiry = new DateTimeOffset(parsed.ValidTo, TimeSpan.Zero);
            return expiry != default;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Grace-cache hit → returns the cached token immediately.
    /// Cache miss → calls <c>RotateAsync</c>, mints a new access token, writes
    /// all three cookies, caches the new access token, and returns it.
    /// Returns <see langword="null"/> and clears cookies on any rotation error.
    /// </summary>
    private async Task<string?> TryRotateAsync(
        HttpContext ctx,
        string rawRefresh,
        IAuthCookieWriter writer,
        JwtSettings jwt
    )
    {
        string graceKey = $"rot:{Sha256Hex(rawRefresh)}";
        if (
            _graceCache.TryGetValue<string>(graceKey, out string? cachedAccess)
            && !string.IsNullOrEmpty(cachedAccess)
        )
        {
            return cachedAccess;
        }

        IRefreshTokenService rts = ctx.RequestServices.GetRequiredService<IRefreshTokenService>();
        ITokenProvider tp = ctx.RequestServices.GetRequiredService<ITokenProvider>();

        var rotation = await rts.RotateAsync(rawRefresh, ctx.RequestAborted);
        if (rotation.IsError)
        {
            _log.LogInformation(
                "Silent refresh failed. RemoteIp={RemoteIp} Error={Error}",
                ctx.Connection.RemoteIpAddress,
                rotation.Errors[0].Code
            );

            writer.ClearAuthCookies(ctx);
            return null;
        }

        var success = rotation.Value;
        var access = await tp.GenerateAccessTokenOnlyAsync(
            success.User,
            success.Family,
            success.DeviceId,
            ctx.RequestAborted
        );

        writer.WriteAccessCookie(ctx, access.Token, access.ExpiresAt);
        writer.WriteRefreshCookie(ctx, success.NewRawToken, success.NewRefreshExpiresAt);
        writer.RefreshCsrfCookie(ctx);

        _graceCache.Set(graceKey, access.Token, TimeSpan.FromSeconds(jwt.RotationGraceSeconds));

        _log.LogDebug(
            "Silent refresh rotated. UserId={UserId} DeviceId={DeviceId} SessionFamilyId={SessionFamilyId}",
            success.User.Id,
            success.DeviceId,
            success.SessionFamilyId
        );

        return access.Token;
    }

    private static string Sha256Hex(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
