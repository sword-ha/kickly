using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/owner")]
[Authorize(Roles = AppRoles.Owner)]
public sealed class OwnerDashboardController : ControllerBase
{
    private readonly IOwnerDashboardService _dashboardService;

    public OwnerDashboardController(IOwnerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<OwnerDashboardStatsDto>> Stats(CancellationToken ct)
    {
        var result = await _dashboardService.GetStatsAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("dashboard/revenue")]
    public async Task<ActionResult<OwnerRevenueDto>> Revenue([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetRevenueAsync(User.GetRequiredUserId(), days, ct);
        return Ok(result);
    }
}
