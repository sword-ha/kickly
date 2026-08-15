using SportsBooking.Domain.Common;

namespace SportsBooking.Domain.Entities;

public sealed class Facility : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<FieldFacility> Fields { get; set; } = new List<FieldFacility>();
}
