using Expense_Tracker.Application.Common;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Infrastructure.Common.Errors;
using Expense_Tracker.Infrastructure.Idenitity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Expense_Tracker.Infrastructure.Services;

public sealed class ExternalAuthService : IExternalAuthService
{
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IAppDbContext db;
    private readonly IIdentityService identityService;
    private readonly string googleClientId;

    public ExternalAuthService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAppDbContext db,
        IIdentityService identityService,
        IConfiguration configuration)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.db = db;
        this.identityService = identityService;

        googleClientId = configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId is missing.");
    }
    public async Task<Result<ExternalAuthDto>> SignInWithGoogleIdTokenAsync(
          string idToken,
          CancellationToken ct)
    {
        try
        {
            GoogleJsonWebSignature.ValidationSettings settings = new()
            {
                Audience = new[] { googleClientId }
            };

            GoogleJsonWebSignature.Payload payload =
                await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            string? email = payload.Email;
            string? firstName = payload.GivenName;
            string? lastName = payload.FamilyName;
            string? fullName = payload.Name;
            string providerKey = payload.Subject;      // unique Google user id
            string provider = "Google";

            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<ExternalAuthDto>(
                    ExternalAuthError.InvalidProviderResponse("Google did not return an email."));
            }

            // 1) Check by external login
            ApplicationUser? identityUser =
                await userManager.FindByLoginAsync(provider, providerKey);

            if (identityUser is null)
            {
                // 2) Try by email
                identityUser = await userManager.FindByEmailAsync(email);

                if (identityUser is null)
                {
                    // CreateRoot Identity user



                    IdentityResult createResult = await userManager.CreateAsync(identityUser);
                    identityUser.EmailConfirmed = true;
                    if (!createResult.Succeeded)
                    {
                        return Result.Failure<ExternalAuthDto>(
                            ExternalAuthError.UserCreationFailed(
                                string.Join(" | ", createResult.Errors.Select(e => e.Description))));
                    }

                }

                // 3) Link external login
                IdentityResult addLogin = await userManager.AddLoginAsync(
                    identityUser,
                    new UserLoginInfo(provider, providerKey, provider));

                if (!addLogin.Succeeded)
                {
                    return Result.Failure<ExternalAuthDto>(
                        ExternalAuthError.LoginLinkFailed(
                            string.Join(" | ", addLogin.Errors.Select(e => e.Description))));
                }
            }


            var dto = new ExternalAuthDto(
                IdentityId: identityUser.Id,
                Email: email,
                FirstName: firstName,
                LastName: lastName,
                UserName: fullName ?? email,
                Provider: provider,
                PhoneNumber: identityUser.PhoneNumber);



            return Result.Success(dto);
        }
        catch (InvalidJwtException ex)
        {
            return Result.Failure<ExternalAuthDto>(
                ExternalAuthError.InvalidProviderResponse($"Invalid Google token: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result.Failure<ExternalAuthDto>(
                ExternalAuthError.Unknown($"Unexpected error: {ex.Message}"));
        }
    }
}