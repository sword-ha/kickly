namespace SportsBooking.Application.DTOs;

public sealed record FacilityDto(int Id, string Name, string Icon, bool IsActive);

public sealed record CreateFacilityRequest(string Name, string Icon);

public sealed record UpdateFacilityRequest(string Name, string Icon, bool IsActive);
