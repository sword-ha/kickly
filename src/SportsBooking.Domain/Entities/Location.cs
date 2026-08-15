using SportsBooking.Domain.Common;

namespace SportsBooking.Domain.Entities;

public sealed class Location : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Governorate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public ICollection<Field> Fields { get; set; } = new List<Field>();
}