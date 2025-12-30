using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
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
    /// <summary>
    /// Generates JWT token without family context (for initial login)
    /// </summary>
    public async Task<Result<AuthDto>> GenerateJwtTokenAsync(
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

    /// <summary>
    /// Generates JWT token with family context (for family selection)
    /// </summary>
    public async Task<Result<AuthDto>> GenerateJwtTokenWithFamilyAsync(
        AuthenticatedUser user,
        string deviceId,
        FamilyContextDto? familyContext,
        CancellationToken ct = default)
    {
        DateTime expiresAt =
            DateTime.UtcNow.AddMinutes(jwt.AccessTokenExpirationMinutes);

        List<Claim> claims = BuildClaims(user, familyContext);

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

        Result revokeResult = await refreshTokens.RevokeActiveTokensAsync(
            user.Id,
            deviceId,
            ct);

        if (revokeResult.IsFailure)
            return Result.Failure<AuthDto>(revokeResult.TryGetError());

        TokenResponse refresh = GenerateRefreshToken();

        Result addResult = await refreshTokens.AddAsync(
            user.Id,
            refresh.Token,
            deviceId,
            ct);

        if (addResult.IsFailure)
            return Result.Failure<AuthDto>(addResult.TryGetError());

        return Result.Success(
            new AuthDto(
                user.Id.ToString(),
                user.Email,
                user.UserName,
                new TokenResponse(accessToken, expiresAt),
                refresh,
                familyContext));
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
            ClockSkew = TimeSpan.Zero
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

    private TokenResponse GenerateRefreshToken()
    {
        string token = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        return new TokenResponse(
            token,
            DateTime.UtcNow.AddDays(jwt.RefreshTokenExpirationDays));
    }

    private static List<Claim> BuildClaims(
        AuthenticatedUser user,
        FamilyContextDto? familyContext)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(CustomClaimTypes.UserId, user.Id.ToString())
        ];

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

    /// <summary>
    /// Extracts family context from claims principal
    /// </summary>
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
