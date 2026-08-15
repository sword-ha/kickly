namespace SportsBooking.Application.Options;

public sealed class LocationOptions
{
    public const string SectionName = "Location";

    /// <summary>Default search radius in kilometers.</summary>
    public double DefaultRadiusKm { get; set; } = 10;

    /// <summary>Maximum allowed search radius in kilometers.</summary>
    public double MaxRadiusKm { get; set; } = 100;
}