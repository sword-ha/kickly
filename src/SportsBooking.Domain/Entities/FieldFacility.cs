namespace SportsBooking.Domain.Entities;

public sealed class FieldFacility
{
    public int FieldId { get; set; }
    public int FacilityId { get; set; }

    public Field Field { get; set; } = null!;
    public Facility Facility { get; set; } = null!;
}
