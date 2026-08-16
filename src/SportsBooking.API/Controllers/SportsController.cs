using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/sports")]
public sealed class SportsController : ControllerBase
{
    private readonly ISportService _sportService;

    public SportsController(ISportService sportService)
    {
        _sportService = sportService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SportDto>>> GetAll(CancellationToken ct)
    {
        var result = await _sportService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SportDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _sportService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<IReadOnlyCollection<SportDto>>> GetAllIncludingInactive(CancellationToken ct)
    {
        var result = await _sportService.GetAllIncludingInactiveAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<SportDto>> Create([FromBody] CreateSportRequest request, CancellationToken ct)
    {
        var result = await _sportService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<SportDto>> Update(int id, [FromBody] UpdateSportRequest request, CancellationToken ct)
    {
        var result = await _sportService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _sportService.DeleteAsync(id, ct);
        return NoContent();
    }
}
