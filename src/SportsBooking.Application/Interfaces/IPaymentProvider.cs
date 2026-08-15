namespace SportsBooking.Application.Interfaces;

public sealed record PaymentChargeRequest(
    int BookingId,
    decimal Amount,
    PaymentProviderChargeOptions Options);

public sealed record PaymentProviderChargeOptions(
    string Currency = "EGP",
    string? Reference = null);

public sealed record PaymentChargeResult(
    bool Succeeded,
    string? TransactionId,
    string? FailureReason);

public sealed record PaymentRefundRequest(
    string TransactionId,
    decimal Amount,
    string? Reason);

public sealed record PaymentRefundResult(
    bool Succeeded,
    string? FailureReason);

public interface IPaymentProvider
{
    Task<PaymentChargeResult> ChargeAsync(PaymentChargeRequest request, CancellationToken ct = default);
    Task<PaymentRefundResult> RefundAsync(PaymentRefundRequest request, CancellationToken ct = default);
}
