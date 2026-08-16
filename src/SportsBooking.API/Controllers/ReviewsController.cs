using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var result = await _reviewService.CreateAsync(User.GetRequiredUserId(), request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> Update(int id, [FromBody] UpdateReviewRequest request, CancellationToken ct)
    {
        var result = await _reviewService.UpdateAsync(User.GetRequiredUserId(), id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _reviewService.DeleteAsync(User.GetRequiredUserId(), id, ct);
        return NoContent();
    }

    [HttpGet("my-reviews")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<ReviewDto>>> MyReviews(CancellationToken ct)
    {
        var result = await _reviewService.GetMyReviewsAsync(User.GetRequiredUserId(), ct);
        return Ok(result);
    }
}