using SportsBooking.Domain.Common;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Domain.Entities;

public sealed class Sport : BaseEntity
{
    public SportType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Field> Fields { get; set; } = new List<Field>();
}