using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Infrastructure.Common.Errors;
using Expense_Tracker.Infrastructure.Idenitity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Expense_Tracker.Infrastructure.Services;

public sealed class ExternalAuthService(SignInManager<ApplicationUser> signInManager
    , UserManager<ApplicationUser> userManager
) : IExternalAuthService
{
    public async Task<Result<AuthenticatedUser>> SignInWithGoogleAsync(CancellationToken ct)
    {
        try
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info is null)
                return Result.Failure<AuthenticatedUser>(
                    ExternalAuthError.InvalidProviderResponse("Google did not return external login info."));

            string? email = info.Principal.FindFirstValue(ClaimTypes.Email);

            string? fullName = info.Principal.FindFirstValue(ClaimTypes.Name);


            if (email is null)
                return Result.Failure<AuthenticatedUser>(
                    ExternalAuthError.InvalidProviderResponse("Google did not return an email."));

            ApplicationUser? identityUser = await userManager.FindByEmailAsync(email);

            if (identityUser is null)
            {
                return Result.Failure<AuthenticatedUser>(
                    ExternalAuthError.UserNotRegistered("This Google account is not registered."));
            }

            var loginRecord = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (loginRecord is null)
            {
                var linkResult = await userManager.AddLoginAsync(identityUser, info);
                if (!linkResult.Succeeded)
                    return Result.Failure<AuthenticatedUser>(
                        ExternalAuthError.LoginLinkFailed("Login With Google Failed"));
            }







            string? role = (await userManager.GetRolesAsync(identityUser)).FirstOrDefault();
            var response = new AuthenticatedUser(identityUser.Id, identityUser.Email, identityUser.UserName, role



            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthenticatedUser>(
                ExternalAuthError.Unknown($"Unexpected error: {ex.Message}"));
        }
    }

}
