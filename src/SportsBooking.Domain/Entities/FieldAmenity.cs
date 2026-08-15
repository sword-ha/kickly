using SportsBooking.Domain.Common;

namespace SportsBooking.Domain.Entities;

public sealed class FieldAmenity : BaseEntity
{
    public int FieldId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public Field Field { get; set; } = null!;
}