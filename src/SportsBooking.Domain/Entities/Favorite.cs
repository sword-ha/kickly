using SportsBooking.Domain.Common;

namespace SportsBooking.Domain.Entities;

public sealed class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public int FieldId { get; set; }

    public User User { get; set; } = null!;
    public Field Field { get; set; } = null!;
}