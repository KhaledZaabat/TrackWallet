using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.PushNotifications.UpdateFcmToken;
using Expense_Tracker.Contracts.Requests.PushNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[Route("api/user-device")]
[ApiController]
[Authorize]
[ApiVersion("1.0")]
public sealed class UserDeviceController(IMessageBus bus) : ControllerBase
{
    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertUserDeviceRequest request,
        CancellationToken cancellationToken
    )
    {
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new UpsertUserDeviceCommand(request.FcmToken),
            cancellationToken
        );

        return result.ToActionResult(this);
    }
}
