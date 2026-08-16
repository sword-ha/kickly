using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminAuditLogsController : ControllerBase
{
    private readonly IAdminAuditLogService _auditLogService;

    public AdminAuditLogsController(IAdminAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _auditLogService.GetLogsAsync(page, pageSize, ct);
        return Ok(result);
    }
}
