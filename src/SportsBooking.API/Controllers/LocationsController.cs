using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/locations")]
public sealed class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LocationDto>>> GetAll(CancellationToken ct)
    {
        var result = await _locationService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDetailsDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _locationService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyCollection<LocationDto>>> Nearby(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] double? radiusKm,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new NearbyLocationsQuery(latitude, longitude, radiusKm, pageSize);
        var result = await _locationService.GetNearbyAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<PagedResult<LocationDto>>> GetAllPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _locationService.GetPagedAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<LocationDto>> Create([FromBody] CreateLocationRequest request, CancellationToken ct)
    {
        var result = await _locationService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<LocationDto>> Update(int id, [FromBody] AdminUpdateLocationRequest request, CancellationToken ct)
    {
        var result = await _locationService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _locationService.DeleteAsync(id, ct);
        return NoContent();
    }
}
