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
public sealed class OwnerBookingsController : ControllerBase
{
    private readonly IOwnerBookingService _ownerBookingService;

    public OwnerBookingsController(IOwnerBookingService ownerBookingService)
    {
        _ownerBookingService = ownerBookingService;
    }

    [HttpGet("bookings/field/{fieldId:int}")]
    public async Task<ActionResult<IReadOnlyCollection<OwnerBookingDto>>> GetFieldBookings(int fieldId, CancellationToken ct)
    {
        var result = await _ownerBookingService.GetFieldBookingsAsync(User.GetRequiredUserId(), fieldId, ct);
        return Ok(result);
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<OwnerBookingDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _ownerBookingService.GetByIdAsync(User.GetRequiredUserId(), id, ct);
        return Ok(result);
    }

    [HttpPut("bookings/{id:int}/status")]
    public async Task<ActionResult<OwnerBookingDto>> UpdateStatus(int id, [FromBody] UpdateOwnerBookingStatusRequest request, CancellationToken ct)
    {
        var result = await _ownerBookingService.UpdateStatusAsync(User.GetRequiredUserId(), id, request, ct);
        return Ok(result);
    }
}
