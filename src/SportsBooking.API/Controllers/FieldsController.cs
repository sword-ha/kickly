using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;
using SportsBooking.API.Extensions;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/fields")]
public sealed class FieldsController : ControllerBase
{
    private readonly IFieldService _fieldService;
    private readonly IOwnerFieldService _ownerFieldService;

    public FieldsController(IFieldService fieldService, IOwnerFieldService ownerFieldService)
    {
        _fieldService = fieldService;
        _ownerFieldService = ownerFieldService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FieldListItemDto>>> GetAll(CancellationToken ct)
    {
        var result = await _fieldService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<ActionResult<IReadOnlyCollection<FieldManagementDto>>> MyFields(CancellationToken ct)
    {
        var result = await _ownerFieldService.GetMyFieldsAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FieldDetailsDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _fieldService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<FieldListItemDto>>> Search(
        [FromQuery] int? sportId,
        [FromQuery] SportType? sportType,
        [FromQuery] FieldType? fieldType,
        [FromQuery] string? city,
        [FromQuery] decimal? minPricePerHour,
        [FromQuery] decimal? maxPricePerHour,
        [FromQuery] decimal? minRating,
        [FromQuery] DateOnly? date,
        [FromQuery] string? facilityIds,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        [FromQuery] double? radiusKm,
        [FromQuery] SortBy sortBy = SortBy.Rating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var ids = ParseIntList(facilityIds);
        var query = new SearchFieldsQuery(
            sportId, sportType, fieldType, city, minPricePerHour, maxPricePerHour, minRating, date,
            ids, latitude, longitude, radiusKm, sortBy, page, pageSize);

        var result = await _fieldService.SearchAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyCollection<FieldListItemDto>>> Nearby(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] double? radiusKm,
        [FromQuery] int? sportId,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new NearbyFieldsQuery(latitude, longitude, radiusKm, sportId, 1, pageSize);
        var result = await _fieldService.GetNearbyAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<IReadOnlyCollection<FieldListItemDto>>> TopRated(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double? radiusKm,
        CancellationToken ct)
    {
        var result = await _fieldService.GetTopRatedAsync(latitude, longitude, radiusKm, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/availability")]
    public async Task<ActionResult<FieldAvailabilityDto>> Availability(int id, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await _fieldService.GetAvailabilityAsync(id, date, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<IReadOnlyCollection<ReviewDto>>> Reviews(int id, CancellationToken ct)
    {
        var result = await _fieldService.GetReviewsAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<ActionResult<FieldManagementDto>> Create([FromBody] CreateFieldRequest request, CancellationToken ct)
    {
        var result = await _ownerFieldService.CreateAsync(User.GetRequiredUserId(), request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<ActionResult<FieldManagementDto>> Update(int id, [FromBody] UpdateFieldRequest request, CancellationToken ct)
    {
        var result = await _ownerFieldService.UpdateAsync(User.GetRequiredUserId(), id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _ownerFieldService.DeleteAsync(User.GetRequiredUserId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/availability")]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<ActionResult<FieldAvailabilityDto>> SetAvailability(int id, [FromBody] SetAvailabilityRequest request, CancellationToken ct)
    {
        var result = await _ownerFieldService.SetAvailabilityAsync(User.GetRequiredUserId(), id, request, ct);
        return Ok(result);
    }

    [HttpPut("{id:int}/availability/{availabilityId:int}")]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<ActionResult<FieldAvailabilityDto>> UpdateAvailability(int id, int availabilityId, [FromBody] UpdateAvailabilityRequest request, CancellationToken ct)
    {
        var result = await _ownerFieldService.UpdateAvailabilityAsync(User.GetRequiredUserId(), id, availabilityId, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}/availability/{availabilityId:int}")]
    [Authorize(Roles = AppRoles.Owner)]
    public async Task<IActionResult> DeleteAvailability(int id, int availabilityId, CancellationToken ct)
    {
        await _ownerFieldService.DeleteAvailabilityAsync(User.GetRequiredUserId(), id, availabilityId, ct);
        return NoContent();
    }

    private static IReadOnlyCollection<int>? ParseIntList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var ids = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .Distinct()
            .ToList();

        return ids.Count == 0 ? null : ids;
    }
}
