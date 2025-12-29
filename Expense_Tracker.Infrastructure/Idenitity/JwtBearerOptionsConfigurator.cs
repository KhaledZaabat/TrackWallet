using Expense_Tracker.Application.Common.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public sealed class JwtBearerOptionsConfigurator
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtSettings jwt;

    public JwtBearerOptionsConfigurator(IOptions<JwtSettings> jwtOptions)
    {
        jwt = jwtOptions.Value;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
            return;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };


    }

    public void Configure(JwtBearerOptions options) { }
}