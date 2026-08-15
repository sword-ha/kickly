using SportsBooking.Domain.Common;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Domain.Entities;

public sealed class Field : BaseEntity
{
    public int SportId { get; set; }
    public int LocationId { get; set; }
    public int? OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public FieldType FieldType { get; set; } = FieldType.Outdoor;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal DayPricePerHour { get; set; }
    public decimal NightPricePerHour { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsApproved { get; set; } = true;
    public DateTime? ApprovedAtUtc { get; set; }

    public Sport Sport { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public User? Owner { get; set; }
    public ICollection<FieldImage> Images { get; set; } = new List<FieldImage>();
    public ICollection<FieldAmenity> Amenities { get; set; } = new List<FieldAmenity>();
    public ICollection<FieldFacility> Facilities { get; set; } = new List<FieldFacility>();
    public ICollection<FieldAvailability> Availability { get; set; } = new List<FieldAvailability>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}

public sealed class FieldImage : BaseEntity
{
    public int FieldId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public Field Field { get; set; } = null!;
}