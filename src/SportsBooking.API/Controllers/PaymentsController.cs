using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBooking.API.Extensions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.CreateAsync(User.GetRequiredUserId(), request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await _paymentService.GetByIdAsync(User.GetRequiredUserId(), id, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/status")]
    public async Task<ActionResult<PaymentStatusResponse>> GetStatus(int id, CancellationToken ct)
    {
        var result = await _paymentService.GetStatusAsync(User.GetRequiredUserId(), id, ct);
        return Ok(result);
    }

    [HttpGet("my-payments")]
    public async Task<ActionResult<PagedResult<PaymentResponse>>> MyPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetUserPaymentsAsync(User.GetRequiredUserId(), page, pageSize, ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/refund")]
    public async Task<ActionResult<PaymentResponse>> Refund(int id, [FromBody] RefundPaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.RefundAsync(User.GetRequiredUserId(), id, request, ct);
        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentResponse>> Webhook([FromBody] PaymentWebhookRequest request, CancellationToken ct)
    {
        // Provider callbacks must never be authenticated as a regular user.
        var result = await _paymentService.ProcessWebhookAsync(request, ct);
        return Ok(result);
    }
}
