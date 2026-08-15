using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _userService;

    public AdminUsersController(IAdminUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _userService.GetUsersAsync(page, pageSize, search, ct);
        return Ok(result);
    }

    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<AdminUserDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<ActionResult<AdminUserDto>> SetStatus(int id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        var result = await _userService.SetStatusAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _userService.DeleteAsync(id, ct);
        return NoContent();
    }
}
