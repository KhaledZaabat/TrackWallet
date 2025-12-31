using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.PushNotifications.UpdateFcmToken;
using Expense_Tracker.Contracts.Requests.PushNotifications;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[Route("api/user-device")]
[ApiController]
[Authorize]
[ApiVersion("1.0")]

public sealed class UserDeviceController(ISender sender) : ControllerBase
{
    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertUserDeviceRequest request,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new UpsertUserDeviceCommand(
                request.FcmToken
            ),
            cancellationToken);

        return result.ToActionResult(HttpContext);
    }
}