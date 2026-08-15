using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationSummaryDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _notificationService.GetAsync(User.GetRequiredUserId(), page, pageSize, ct);
        return Ok(result);
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(int id, CancellationToken ct)
    {
        var result = await _notificationService.MarkReadAsync(User.GetRequiredUserId(), id, ct);
        return Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _notificationService.MarkAllReadAsync(User.GetRequiredUserId(), ct);
        return NoContent();
    }
}
