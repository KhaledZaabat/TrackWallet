using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Auth;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Features;
using Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;
using Expense_Tracker.Application.Features.Identity.Commands.ForgotPassword;
using Expense_Tracker.Application.Features.Identity.Commands.Logout;
using Expense_Tracker.Application.Features.Identity.Commands.ResendConfirmation;
using Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;
using Expense_Tracker.Application.Features.Login;
using Expense_Tracker.Application.Features.Refresh;
using Expense_Tracker.Application.Features.Register;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Requests.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Expense_Tracker.App.Controllers;
[ApiController]
[Route("api/identity")]
[Tags("Identity")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public sealed class IdentityController(
    IMessageBus bus,
    IAuthCookieWriter authCookies,
    IOptionsMonitor<AuthCookieOptions> cookieOptions
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Authenticates a user by email.")]
    [EndpointDescription(
        "Validates email and password and returns a JWT + Refresh token pair as http only cookie  ."
    )]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        LoginCommand command = new(
            request.Email,
            request.Password,
            request.DeviceId,
            request.FcmToken
        );

       ErrorOr<AuthCommandResult> result = await bus.InvokeAsync<ErrorOr<AuthCommandResult>>(
            command,
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        AuthCommandResult value = result.Value;

        authCookies.WriteAccessCookie(HttpContext, value.AccessToken, value.AccessExpiresAt);
        authCookies.WriteRefreshCookie(HttpContext, value.RefreshToken, value.RefreshExpiresAt);
        authCookies.IssueCsrfCookie(HttpContext);

        return value.Response;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Register user by email")]
    [EndpointDescription(
        "Creates a new user account using email credentials and sends a verification OTP."
    )]
    [EndpointName("Register")]
    public async Task<IActionResult> Register(
        [FromForm] RegisterRequest request,
        CancellationToken ct
    )
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.UserName,
            request.FullName,
            request.BirthDate,
            request.IsMale,
            request.ProfileImage
        );

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
    }
    [AllowAnonymous]
    [HttpPost("confirm-account/otp/resend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Resends the confirmation OTP code to email .")]
    [EndpointDescription("Allows users to request another OTP for confirming their account.")]
    [EndpointName("ResendConfirmationOtp")]
    public async Task<IActionResult> ResendConfirmationOtp(
        [FromBody] ResendConfirmationRequest request,
        CancellationToken ct
    )
    {
        var command = new ResendConfirmationCommand(request.Email);

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
    }
    [AllowAnonymous]
    [HttpPost("confirm-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Confirms a user account using the magic-link token.")]
    [EndpointDescription(
        "Validates the email-confirmation token (issued by ASP.NET Identity and emailed as a magic link) and activates the user account."
    )]
    [EndpointName("ConfirmAccount")]
    public async Task<IActionResult> ConfirmAccount(
        [FromBody] ConfirmAccountRequest request,
        CancellationToken ct
    )
    {
        var command = new ConfirmAccountCommand(request.Email, request.Token);

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
    }
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Refreshes JWT token pair.")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct
    )
    {
        AuthCookieOptions cookieOpts = cookieOptions.CurrentValue;

        // R15.1, R15.3 — raw refresh MUST come from the cookie; the body is ignored.
        string? rawRefresh = HttpContext.Request.Cookies[cookieOpts.RefreshCookieName];
        if (string.IsNullOrEmpty(rawRefresh))
            return Unauthorized();

        RefreshTokenCommand command = new(rawRefresh, request.FcmToken);

        ErrorOr<AuthCommandResult> result = await bus.InvokeAsync<ErrorOr<AuthCommandResult>>(
            command,
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        AuthCommandResult value = result.Value;

        // R2.2, R3.2, R12.2 — rotate cookies and refresh CSRF on success.
        authCookies.WriteAccessCookie(HttpContext, value.AccessToken, value.AccessExpiresAt);
        authCookies.WriteRefreshCookie(HttpContext, value.RefreshToken, value.RefreshExpiresAt);
        authCookies.RefreshCsrfCookie(HttpContext);

        return value.Response;
    }
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Sends a password-reset magic link.")]
    [EndpointDescription(
        "Always returns 200 — the response is intentionally identical whether the email exists or not, so callers can't enumerate accounts. If a matching, confirmed account exists, a magic-link email is sent."
    )]
    [EndpointName("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ResetPasswordOtpSendRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordOtpSendCommand(request.Email);
        ErrorOr<Success> res = await bus.InvokeAsync<ErrorOr<Success>>(command, cancellationToken);
        return res.ToActionResult(this);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Resets the user's password using the magic-link token.")]
    [EndpointDescription(
        "Validates the password-reset token and applies the new password atomically. The user's SecurityStamp rotates on success, invalidating any other outstanding reset/confirmation tokens."
    )]
    [EndpointName("ResetPassword")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new ResetPasswordCommand(
            Email: request.Email,
            Token: request.Token,
            NewPassword: request.NewPassword,
            UserIpAddress: userIpAddress);

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, cancellationToken);
        return result.ToActionResult(this);
    }
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Logs out the current user.")]
    [EndpointDescription("Revokes the user's latest refresh token, invalidating their session.")]
    [EndpointName("Logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        HttpContext.Items["AuthLogoutInProgress"] = true;

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new LogoutCommand(request.DeviceId, request.FcmToken),
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        authCookies.ClearAuthCookies(HttpContext);

        return Ok();
    }
[Authorize]
[HttpGet("me")]
[ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[EndpointSummary("Returns the authenticated user's profile.")]
[EndpointName("GetMe")]
public async Task<ActionResult<MeResponse>> Me(
    [FromServices] IUserContext userContext,
    CancellationToken ct)
{
    if (userContext.UserId is not { } userId)
        return Unauthorized();

    ErrorOr<MeResult> result = await bus.InvokeAsync<ErrorOr<MeResult>>(
        new GetMeQuery(userId),
        ct
    );

    if (result.IsError)
        return this.Problem(result.Errors);

    MeResult value = result.Value;

    return new MeResponse(
        value.Id,
        value.Email,
        value.UserName,
        value.FullName,
        value.BirthDate,
        value.IsMale,
        value.ProfileImageUrl
    );
}
}

