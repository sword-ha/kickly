using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

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
}
