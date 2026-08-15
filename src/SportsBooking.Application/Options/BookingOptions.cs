namespace SportsBooking.Application.Options;

public sealed class BookingOptions
{
    public const string SectionName = "Booking";

    /// <summary>Maximum booking duration in hours (default 4).</summary>
    public int MaxDurationHours { get; set; } = 4;

    /// <summary>Minimum booking duration in hours.</summary>
    public int MinDurationHours { get; set; } = 1;
}