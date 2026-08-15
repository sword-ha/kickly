using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record CreatePaymentRequest(int BookingId, PaymentMethod Method);

public sealed record PaymentResponse(
    int Id,
    int BookingId,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    string? Provider,
    string? TransactionId,
    string? FailureReason,
    DateTime? PaidAtUtc,
    DateTime? RefundedAtUtc,
    DateTime CreatedAtUtc);

public sealed record PaymentStatusResponse(int PaymentId, PaymentStatus Status, string? TransactionId);

public sealed record PaymentWebhookRequest(string TransactionId, PaymentStatus Status, string? Provider = null);

public sealed record RefundPaymentRequest(string Reason);
