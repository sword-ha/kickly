using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public sealed class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FavoriteDto>>> GetAll(CancellationToken ct)
    {
        var result = await _favoriteService.GetAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{fieldId:int}/exists")]
    public async Task<ActionResult<bool>> Exists(int fieldId, CancellationToken ct)
    {
        var result = await _favoriteService.ExistsAsync(User.GetRequiredUserId(), fieldId, ct);
        return Ok(result);
    }

    [HttpPost("{fieldId:int}")]
    public async Task<IActionResult> Add(int fieldId, CancellationToken ct)
    {
        await _favoriteService.AddAsync(User.GetRequiredUserId(), fieldId, ct);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("{fieldId:int}")]
    public async Task<IActionResult> Remove(int fieldId, CancellationToken ct)
    {
        await _favoriteService.RemoveAsync(User.GetRequiredUserId(), fieldId, ct);
        return NoContent();
    }
}