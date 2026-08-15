using SportsBooking.Domain.Common;

namespace SportsBooking.Domain.Entities;

public sealed class Review : BaseEntity
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int FieldId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public Booking Booking { get; set; } = null!;
    public User User { get; set; } = null!;
    public Field Field { get; set; } = null!;
}