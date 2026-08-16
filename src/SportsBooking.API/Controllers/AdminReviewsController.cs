using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminReviewsController : ControllerBase
{
    private readonly IAdminReviewService _reviewService;

    public AdminReviewsController(IAdminReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminReviewDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsAsync(page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminReviewDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _reviewService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _reviewService.DeleteAsync(id, ct);
        return NoContent();
    }
}
