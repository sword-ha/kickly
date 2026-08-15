using SportsBooking.Domain.Common;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Domain.Entities;

public sealed class Payment : BaseEntity
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? Provider { get; set; }
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? RefundedAtUtc { get; set; }

    public Booking Booking { get; set; } = null!;
}
