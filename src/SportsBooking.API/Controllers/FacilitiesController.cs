using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/facilities")]
public sealed class FacilitiesController : ControllerBase
{
    private readonly IFacilityService _facilityService;

    public FacilitiesController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FacilityDto>>> GetAll(CancellationToken ct)
    {
        var result = await _facilityService.GetAllActiveAsync(ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<IReadOnlyCollection<FacilityDto>>> GetAllAdmin(CancellationToken ct)
    {
        var result = await _facilityService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FacilityDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _facilityService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<FacilityDto>> Create([FromBody] CreateFacilityRequest request, CancellationToken ct)
    {
        var result = await _facilityService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<FacilityDto>> Update(int id, [FromBody] UpdateFacilityRequest request, CancellationToken ct)
    {
        var result = await _facilityService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _facilityService.DeleteAsync(id, ct);
        return NoContent();
    }
}
