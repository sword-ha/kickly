using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminReportsController : ControllerBase
{
    private readonly IAdminReportService _reportService;

    public AdminReportsController(IAdminReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("reports")]
    public async Task<ActionResult<AdminReportDto>> GetReport(CancellationToken ct)
    {
        var result = await _reportService.GetReportAsync(ct);
        return Ok(result);
    }
}
