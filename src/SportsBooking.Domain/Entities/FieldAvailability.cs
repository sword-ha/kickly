using SportsBooking.Domain.Common;

namespace SportsBooking.Domain.Entities;

public sealed class FieldAvailability : BaseEntity
{
    public int FieldId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; }

    public Field Field { get; set; } = null!;
}