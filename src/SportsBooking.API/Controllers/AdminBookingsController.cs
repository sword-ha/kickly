using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminBookingsController : ControllerBase
{
    private readonly IAdminBookingService _bookingService;

    public AdminBookingsController(IAdminBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<PagedResult<AdminBookingDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] BookingStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _bookingService.GetBookingsAsync(page, pageSize, status, ct);
        return Ok(result);
    }

    [HttpGet("bookings/{id:int}")]
    public async Task<ActionResult<AdminBookingDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _bookingService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("bookings/{id:int}/cancel")]
    public async Task<ActionResult<AdminBookingDto>> Cancel(int id, [FromBody] CancelBookingRequest? request, CancellationToken ct)
    {
        var result = await _bookingService.CancelAsync(id, request?.Reason, ct);
        return Ok(result);
    }
}
