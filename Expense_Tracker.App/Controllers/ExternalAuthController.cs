using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Requests.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Infrastructure.Idenitity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[Route("api/identity")]
[Tags("Identity - External Auth")]
[Produces("application/json")]
[ApiVersion("1.0")]
public sealed class ExternalAuthController(
    IAuthCookieWriter authCookieWriter,
    ISender sender,
    SignInManager<ApplicationUser> signInManager
) : ControllerBase
{
    private const string GoogleProvider = "Google";



    [HttpPost("login/google/mobile")]
    [EndpointName("LoginWithGoogleMobile")]
    [EndpointSummary("Google login for mobile clients.")]
    [EndpointDescription("Accepts a Google ID token from mobile and returns Expense_Tracker JWT + refresh token.")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> LoginWithGoogleMobile(
        [FromBody] GoogleMobileLoginRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new { error = "idToken is required." });

        if (string.IsNullOrWhiteSpace(request.DeviceId))
            return BadRequest(new { error = "deviceId is required." });

        Result<AuthResponse> result =
            await sender.Send(new GoogleMobileLoginCommand(request.IdToken, request.DeviceId, request.FcmToken), ct);

        return result.ToActionResult<AuthResponse>(HttpContext);
    }


}
