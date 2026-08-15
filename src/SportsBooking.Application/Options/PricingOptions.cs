namespace SportsBooking.Application.Options;

public sealed class PricingOptions
{
    public const string SectionName = "Pricing";

    /// <summary>Base time (hour) when the day period starts.</summary>
    public int DayStartHour { get; set; } = 8;

    /// <summary>Base time (hour) when the night period starts.</summary>
    public int NightStartHour { get; set; } = 18;

    public bool IsNight(TimeOnly time) => time.Hour >= NightStartHour || time.Hour < DayStartHour;
}