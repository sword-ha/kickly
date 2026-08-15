using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminFieldsController : ControllerBase
{
    private readonly IAdminFieldService _fieldService;

    public AdminFieldsController(IAdminFieldService fieldService)
    {
        _fieldService = fieldService;
    }

    [HttpGet("fields")]
    public async Task<ActionResult<PagedResult<AdminFieldDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? pendingOnly = null,
        CancellationToken ct = default)
    {
        var result = await _fieldService.GetFieldsAsync(page, pageSize, pendingOnly, ct);
        return Ok(result);
    }

    [HttpGet("fields/{id:int}")]
    public async Task<ActionResult<AdminFieldDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _fieldService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("fields/{id:int}/approval")]
    public async Task<ActionResult<AdminFieldDto>> SetApproval(int id, [FromBody] SetFieldApprovalRequest request, CancellationToken ct)
    {
        var result = await _fieldService.SetApprovalAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPut("fields/{id:int}/status")]
    public async Task<ActionResult<AdminFieldDto>> SetStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        var result = await _fieldService.SetStatusAsync(id, request, ct);
        return Ok(result);
    }
}
