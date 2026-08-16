using Microsoft.Extensions.Options;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class AdminPaymentService : IAdminPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly PaymentOptions _paymentOptions;

    public AdminPaymentService(
        IPaymentRepository paymentRepository,
        IPaymentProvider paymentProvider,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IOptions<PaymentOptions> paymentOptions)
    {
        _paymentRepository = paymentRepository;
        _paymentProvider = paymentProvider;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _paymentOptions = paymentOptions.Value;
    }

    public async Task<PagedResult<AdminPaymentDto>> GetPaymentsAsync(int page, int pageSize, PaymentStatus? status, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _paymentRepository.GetPagedAsync(page, pageSize, status, ct);

        return new PagedResult<AdminPaymentDto>(
            items.Select(Map).ToList(),
            total,
            page,
            pageSize);
    }

    public async Task<AdminPaymentDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Payment was not found.");

        return Map(payment);
    }

    public async Task<AdminPaymentDto> RefundAsync(int id, RefundPaymentRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Payment was not found.");

        if (payment.Status != PaymentStatus.Paid)
        {
            throw new ConflictException("Only paid payments can be refunded.");
        }

        var result = await _paymentProvider.RefundAsync(
            new PaymentRefundRequest(payment.TransactionId ?? string.Empty, payment.Amount, request.Reason),
            ct);

        if (!result.Succeeded)
        {
            throw new PaymentFailedException(result.FailureReason ?? "Refund failed.");
        }

        payment.Status = PaymentStatus.Refunded;
        payment.RefundedAtUtc = DateTime.UtcNow;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await _paymentRepository.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            null, "RefundPayment", nameof(Domain.Entities.Payment), id.ToString(), request.Reason, ct);

        await _notificationService.CreateAsync(
            payment.Booking.UserId,
            "Payment refunded",
            $"Your payment for booking #{payment.BookingId} was refunded.",
            NotificationType.PaymentRefunded, ct);

        return Map(payment);
    }

    private static AdminPaymentDto Map(Domain.Entities.Payment p)
        => new(
            p.Id,
            p.BookingId,
            p.Booking.Field.Id,
            p.Booking.Field.Name,
            p.Booking.User.Id,
            $"{p.Booking.User.FirstName} {p.Booking.User.LastName}".Trim(),
            p.Amount,
            p.Method,
            p.Status,
            p.Provider,
            p.TransactionId,
            p.FailureReason,
            p.PaidAtUtc,
            p.RefundedAtUtc,
            p.CreatedAtUtc);
}
