using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminPaymentsController : ControllerBase
{
    private readonly IAdminPaymentService _paymentService;

    public AdminPaymentsController(IAdminPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminPaymentDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PaymentStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetPaymentsAsync(page, pageSize, status, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminPaymentDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _paymentService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/refund")]
    public async Task<ActionResult<AdminPaymentDto>> Refund(int id, [FromBody] RefundPaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.RefundAsync(id, request, ct);
        return Ok(result);
    }
}
