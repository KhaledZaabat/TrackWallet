using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using ErrorOr;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public sealed class TokenProvider(
    IRefreshTokenService refreshTokens,
    JwtSettings jwt
) : ITokenProvider, IScopedService
{
    public async Task<ErrorOr<AuthDto>> GenerateJwtTokenAsync(
        AuthenticatedUser user,
        string deviceId,
        CancellationToken ct = default)
    {
        return await GenerateJwtTokenWithFamilyAsync(
            user,
            deviceId,
            familyContext: null,
            ct);
    }

    public async Task<ErrorOr<AuthDto>> GenerateJwtTokenWithFamilyAsync(
        AuthenticatedUser user,
        string deviceId,
        FamilyContextDto? familyContext,
        CancellationToken ct = default)
    {
        DateTime expiresAt =
            DateTime.UtcNow.AddMinutes(jwt.AccessTokenExpirationMinutes);

        List<Claim> claims = BuildClaims(user, deviceId, familyContext);

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };

        JwtSecurityTokenHandler handler = new();
        SecurityToken securityToken = handler.CreateToken(descriptor);
        string accessToken = handler.WriteToken(securityToken);

        ErrorOr<Success> revokeResult = await refreshTokens.RevokeActiveTokensAsync(
            user.Id,
            deviceId,
            ct);

        if (revokeResult.IsError)
            return revokeResult.Errors[0];

        Guid sessionFamilyId = Guid.CreateVersion7();
        DateTimeOffset originalIssuedAt = DateTimeOffset.UtcNow;
        string rawRefresh = GenerateOpaqueRefreshToken();
        DateTimeOffset refreshExpiresAt =
            originalIssuedAt.AddDays(jwt.RefreshTokenExpirationDays);

        ErrorOr<Success> addResult = await refreshTokens.AddNewSessionAsync(
            user.Id,
            rawRefresh,
            deviceId,
            sessionFamilyId,
            originalIssuedAt,
            ct);

        if (addResult.IsError)
            return addResult.Errors[0];

        return new AuthDto(
                user.Id.ToString(),
                user.Email,
                user.UserName,
                new TokenResponse(accessToken, expiresAt),
                new TokenResponse(rawRefresh, refreshExpiresAt.UtcDateTime),
                familyContext);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds)
        };

        JwtSecurityTokenHandler handler = new();

        try
        {
            ClaimsPrincipal principal =
                handler.ValidateToken(token, parameters, out SecurityToken validated);

            if (validated is not JwtSecurityToken jwtToken ||
                jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Task<AccessTokenResult> GenerateAccessTokenOnlyAsync(
        AuthenticatedUser user,
        FamilyContextDto? family,
        string deviceId,
        CancellationToken ct)
    {
        DateTime expiresAt =
            DateTime.UtcNow.AddMinutes(jwt.AccessTokenExpirationMinutes);

        List<Claim> claims = BuildClaims(user, deviceId, family);

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };

        JwtSecurityTokenHandler handler = new();
        SecurityToken securityToken = handler.CreateToken(descriptor);
        string accessToken = handler.WriteToken(securityToken);

        return Task.FromResult(
            new AccessTokenResult(
                accessToken,
                new DateTimeOffset(expiresAt, TimeSpan.Zero)));
    }

    private static string GenerateOpaqueRefreshToken()
        => Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static List<Claim> BuildClaims(
        AuthenticatedUser user,
        string deviceId,
        FamilyContextDto? familyContext)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(CustomClaimTypes.UserId, user.Id.ToString())
        ];

        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            claims.Add(new Claim(CustomClaimTypes.DeviceId, deviceId));
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new(CustomClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(CustomClaimTypes.UserName, user.UserName));
        }

        // Add family context claims if provided
        if (familyContext is not null)
        {
            claims.Add(new Claim(CustomClaimTypes.FamilyId, familyContext.FamilyId.ToString()));
            claims.Add(new Claim(CustomClaimTypes.FamilyName, familyContext.FamilyName));
            claims.Add(new Claim(CustomClaimTypes.IsParent, familyContext.IsParent.ToString().ToLower()));
        }

        return claims;
    }

    public static FamilyContextDto? GetFamilyContext(ClaimsPrincipal principal)
    {
        var familyId = principal.FindFirst(CustomClaimTypes.FamilyId)?.Value;
        var familyName = principal.FindFirst(CustomClaimTypes.FamilyName)?.Value;
        var isParentClaim = principal.FindFirst(CustomClaimTypes.IsParent)?.Value;

        if (string.IsNullOrWhiteSpace(familyId))
            return null;

        bool isParent = bool.TryParse(isParentClaim, out var parsed) && parsed;
        Guid familyIdguid = Guid.Parse(familyId);
        return new FamilyContextDto(
            familyIdguid,
            familyName ?? string.Empty,
            isParent);
    }
}
