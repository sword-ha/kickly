using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace SportsBooking.Application.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentProvider _paymentProvider;
    private readonly INotificationService _notificationService;
    private readonly PaymentOptions _paymentOptions;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        IPaymentProvider paymentProvider,
        INotificationService notificationService,
        IOptions<PaymentOptions> paymentOptions)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _paymentProvider = paymentProvider;
        _notificationService = notificationService;
        _paymentOptions = paymentOptions.Value;
    }

    public async Task<PaymentResponse> CreateAsync(int userId, CreatePaymentRequest request, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.UserId != userId)
        {
            throw new ForbiddenException("You can only pay for your own bookings.");
        }

        if (!booking.Field.IsActive)
        {
            throw new ConflictException("This field is no longer active.");
        }

        // Server-side price: never trust an amount coming from the client.
        var amount = booking.TotalPrice;
        if (amount <= 0)
        {
            throw new ValidationDomainException("Booking total must be greater than zero.");
        }

        // Only bookings awaiting payment can be paid.
        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new ConflictException("This booking is not awaiting payment.");
        }

        var latest = await _paymentRepository.GetLatestByBookingIdAsync(booking.Id, ct);
        if (latest is not null && latest.Status == PaymentStatus.Pending)
        {
            throw new ConflictException("A payment for this booking is already being processed.");
        }

        var result = await _paymentProvider.ChargeAsync(
            new PaymentChargeRequest(booking.Id, amount, new PaymentProviderChargeOptions(Currency: _paymentOptions.Currency)),
            ct);

        if (!result.Succeeded)
        {
            await RecordFailedPaymentAsync(booking, request.Method, result.FailureReason, ct);
            await _notificationService.CreateAsync(
                booking.UserId,
                "Payment failed",
                $"Your payment for booking #{booking.Id} could not be completed. Please try again.",
                NotificationType.PaymentFailed, ct);
            throw new PaymentFailedException(result.FailureReason ?? "Payment failed.");
        }

        var payment = await RecordPaidPaymentAsync(booking, request.Method, result.TransactionId, ct);

        // Successful payment confirms the booking.
        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await _paymentRepository.SaveChangesAsync(ct);

        await _notificationService.CreateAsync(
            booking.UserId,
            "Payment successful",
            $"Your payment of {amount} {_paymentOptions.Currency} was received. Booking confirmed.",
            NotificationType.PaymentSucceeded, ct);

        return Map(payment, booking);
    }

    public async Task<PaymentResponse> GetByIdAsync(int userId, int paymentId, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException("Payment was not found.");

        if (payment.Booking.UserId != userId)
        {
            throw new ForbiddenException("You can only view your own payments.");
        }

        return Map(payment, payment.Booking);
    }

    public async Task<PaymentStatusResponse> GetStatusAsync(int userId, int paymentId, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException("Payment was not found.");

        if (payment.Booking.UserId != userId)
        {
            throw new ForbiddenException("You can only view your own payments.");
        }

        return new PaymentStatusResponse(payment.Id, payment.Status, payment.TransactionId);
    }

    public async Task<PagedResult<PaymentResponse>> GetUserPaymentsAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _paymentRepository.GetByUserIdPagedAsync(userId, page, pageSize, ct);

        return new PagedResult<PaymentResponse>(
            items.Select(p => Map(p, p.Booking)).ToList(),
            total,
            page,
            pageSize);
    }

    public async Task<PaymentResponse> ProcessWebhookAsync(PaymentWebhookRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId, ct)
            ?? throw new NotFoundException("Payment was not found.");

        if (payment.Status is PaymentStatus.Refunded or PaymentStatus.Cancelled)
        {
            return Map(payment, payment.Booking);
        }

        if (payment.Status == PaymentStatus.Paid || payment.Status == request.Status)
        {
            return Map(payment, payment.Booking);
        }

        if (request.Status == PaymentStatus.Paid)
        {
            payment.Status = PaymentStatus.Paid;
            payment.PaidAtUtc = DateTime.UtcNow;
            payment.Booking.Status = BookingStatus.Confirmed;
            payment.Booking.UpdatedAtUtc = DateTime.UtcNow;
        }
        else if (request.Status == PaymentStatus.Failed)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Provider reported a failed payment.";
            payment.Booking.Status = BookingStatus.PendingPayment;
            payment.Booking.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            payment.Status = request.Status;
            payment.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _paymentRepository.SaveChangesAsync(ct);
        return Map(payment, payment.Booking);
    }

    public async Task<PaymentResponse> RefundAsync(int userId, int paymentId, RefundPaymentRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException("Payment was not found.");

        if (payment.Booking.UserId != userId)
        {
            throw new ForbiddenException("You can only refund your own payments.");
        }

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

        await _notificationService.CreateAsync(
            payment.Booking.UserId,
            "Payment refunded",
            $"Your payment for booking #{payment.BookingId} was refunded.",
            NotificationType.PaymentRefunded, ct);

        return Map(payment, payment.Booking);
    }

    private async Task<Payment> RecordFailedPaymentAsync(Booking booking, PaymentMethod method, string? failureReason, CancellationToken ct)
    {
        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = booking.TotalPrice,
            Method = method,
            Status = PaymentStatus.Failed,
            Provider = _paymentOptions.Provider,
            FailureReason = failureReason
        };

        await _paymentRepository.AddAsync(payment, ct);
        await _paymentRepository.SaveChangesAsync(ct);
        return payment;
    }

    private async Task<Payment> RecordPaidPaymentAsync(Booking booking, PaymentMethod method, string? transactionId, CancellationToken ct)
    {
        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = booking.TotalPrice,
            Method = method,
            Status = PaymentStatus.Paid,
            Provider = _paymentOptions.Provider,
            TransactionId = transactionId,
            PaidAtUtc = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment, ct);
        await _paymentRepository.SaveChangesAsync(ct);
        return payment;
    }

    private static PaymentResponse Map(Payment payment, Booking booking)
        => new(
            payment.Id,
            payment.BookingId,
            payment.Amount,
            payment.Method,
            payment.Status,
            payment.Provider,
            payment.TransactionId,
            payment.FailureReason,
            payment.PaidAtUtc,
            payment.RefundedAtUtc,
            payment.CreatedAtUtc);
}
