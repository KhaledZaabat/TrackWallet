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
using Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;
using Expense_Tracker.Application.Features.Login;
using Expense_Tracker.Application.Features.Refresh;
using Expense_Tracker.Application.Features.Register;
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
    [EndpointSummary("Confirms a user account using OTP.")]
    [EndpointDescription("Validates OTP for email or phone and activates the user account.")]
    [EndpointName("ConfirmAccount")]
    public async Task<IActionResult> ConfirmAccount(
        [FromBody] ConfirmAccountRequest request,
        CancellationToken ct
    )
    {
        var command = new ConfirmAccountCommand(request.Email, request.Otp);

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
    [HttpPost("reset-password/otp/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Sends OTP to email  for resetting the password.")]
    [EndpointDescription("Generates and sends a password-reset OTP to the provided email .")]
    [EndpointName("SendResetPasswordOtp")]
    public async Task<IActionResult> SendResetPasswordOtp(
        [FromBody] ResetPasswordOtpSendRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new ResetPasswordOtpSendCommand(request.Email);
        ErrorOr<Success> res = await bus.InvokeAsync<ErrorOr<Success>>(command, cancellationToken);
        return res.ToActionResult(this);
    }
    [AllowAnonymous]
    [HttpPost("reset-password/otp/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Verifies OTP for resetting the password.")]
    [EndpointDescription("Checks whether a password-reset OTP is correct and has not expired.")]
    [EndpointName("VerifyResetPasswordOtp")]
    public async Task<IActionResult> VerifyResetPasswordOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new VerifyOtpCommand(request.Email, request.Otp);
        ErrorOr<Success> res = await bus.InvokeAsync<ErrorOr<Success>>(command, cancellationToken);
        return res.ToActionResult(this);
    }
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Resets the user's password.")]
    [EndpointDescription("Allows users to set a new password once OTP verification succeeds.")]
    [EndpointName("ResetPassword")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new ResetPasswordCommand(request.Email, request.NewPassword, userIpAddress);
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
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
        // R14.4 — set the skip marker BEFORE dispatching so SilentRefreshMiddleware
        // does not attempt a rotation on the response path for this request.
        HttpContext.Items["AuthLogoutInProgress"] = true;

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new LogoutCommand(request.DeviceId, request.FcmToken),
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        // R14.2 — clear access + refresh + CSRF with the exact attributes used on write (R22.9).
        authCookies.ClearAuthCookies(HttpContext);

        return Ok();
    }
}
