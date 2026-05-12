using System.Text;
using Expense_Tracker.Application.Common.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Binds <see cref="JwtBearerOptions"/> to TrackWallet's cookie-based authentication
/// transport. The access token is read from the HttpOnly cookie first; the
/// Authorization header is consulted as a fallback only — that fallback is used by
/// <see cref="SilentRefreshMiddleware"/> to present a freshly minted token during the
/// same request that triggered rotation. Clock skew flows from
/// <see cref="JwtSettings.ClockSkewSeconds"/> so every auth component shares one tolerance.
/// </summary>
public sealed class JwtBearerOptionsConfigurator : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtSettings _jwt;
    private readonly AuthCookieOptions _cookies;

    public JwtBearerOptionsConfigurator(
        IOptions<JwtSettings> jwtOptions,
        IOptions<AuthCookieOptions> cookieOptions)
    {
        _jwt = jwtOptions.Value;
        _cookies = cookieOptions.Value;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
            return;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(_jwt.ClockSkewSeconds),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctxEvt =>
            {
                // Prefer the cookie. If it is absent (e.g. the browser evicted it after
                // expiry and SilentRefreshMiddleware has just injected the new token into
                // Authorization), fall through to the handler's default header reader.
                if (ctxEvt.Request.Cookies.TryGetValue(_cookies.AccessCookieName, out string? cookieToken)
                    && !string.IsNullOrEmpty(cookieToken))
                {
                    ctxEvt.Token = cookieToken;
                }

                return Task.CompletedTask;
            },
        };
    }

    public void Configure(JwtBearerOptions options) { }
}
