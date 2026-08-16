using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;

    public AdminNotificationsController(INotificationService notificationService, IAuditLogService auditLogService)
    {
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    [HttpPost("broadcast")]
    public async Task<ActionResult<int>> Broadcast([FromBody] BroadcastNotificationRequest request, CancellationToken ct)
    {
        var result = await _notificationService.BroadcastAsync(request.Title, request.Message, ct);

        await _auditLogService.LogAsync(
            null, "Broadcast", nameof(Domain.Entities.Notification), string.Empty,
            $"\"{request.Title}\" to {result} recipients", ct);

        return Ok(result);
    }
}
