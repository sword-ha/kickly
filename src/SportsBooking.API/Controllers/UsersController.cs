using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBookingService _bookingService;
    private readonly IFavoriteService _favoriteService;

    public UsersController(IUserService userService, IBookingService bookingService, IFavoriteService favoriteService)
    {
        _userService = userService;
        _bookingService = bookingService;
        _favoriteService = favoriteService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken ct)
    {
        var result = await _userService.GetProfileAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateProfileAsync(User.GetRequiredUserId(), request, ct);
        return Ok(result);
    }

    [HttpPut("me/location")]
    public async Task<ActionResult<UserProfileDto>> UpdateLocation([FromBody] UpdateLocationRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateLocationAsync(User.GetRequiredUserId(), request, ct);
        return Ok(result);
    }

    [HttpGet("me/bookings")]
    public async Task<ActionResult<IReadOnlyCollection<BookingDto>>> MyBookings(CancellationToken ct)
    {
        var result = await _bookingService.GetUserBookingsAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("me/favorites")]
    public async Task<ActionResult<IReadOnlyCollection<FavoriteDto>>> MyFavorites(CancellationToken ct)
    {
        var result = await _favoriteService.GetAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }
}