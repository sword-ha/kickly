using SportsBooking.Domain.Common;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Domain.Entities;

public sealed class Booking : BaseEntity
{
    public int UserId { get; set; }
    public int FieldId { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationHours { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public byte[] ConcurrencyStamp { get; set; } = Array.Empty<byte>();

    public User User { get; set; } = null!;
    public Field Field { get; set; } = null!;
    public Review? Review { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}