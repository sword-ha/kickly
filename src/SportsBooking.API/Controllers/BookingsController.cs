using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookingService.CreateAsync(User.GetRequiredUserId(), request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("preview")]
    public async Task<ActionResult<BookingPreviewDto>> Preview([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookingService.PreviewAsync(request.FieldId, request.Date, request.StartTime, request.DurationHours, ct);
        return Ok(result);
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<IReadOnlyCollection<BookingDto>>> MyBookings(CancellationToken ct)
    {
        var result = await _bookingService.GetUserBookingsAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyCollection<BookingDto>>> Upcoming(CancellationToken ct)
    {
        var result = await _bookingService.GetUpcomingAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("past")]
    public async Task<ActionResult<IReadOnlyCollection<BookingDto>>> Past(CancellationToken ct)
    {
        var result = await _bookingService.GetPastAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<BookingStatsDto>> Stats(CancellationToken ct)
    {
        var result = await _bookingService.GetStatsAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _bookingService.GetByIdAsync(User.GetRequiredUserId(), id, ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<BookingDto>> Cancel(int id, [FromBody] CancelBookingRequest? request, CancellationToken ct)
    {
        var result = await _bookingService.CancelAsync(User.GetRequiredUserId(), id, request?.Reason, ct);
        return Ok(result);
    }
}