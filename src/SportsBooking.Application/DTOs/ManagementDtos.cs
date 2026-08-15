using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record CreateSportRequest(SportType Type, string Name, string Slug, string Description, bool IsActive = true);

public sealed record UpdateSportRequest(SportType Type, string Name, string Slug, string Description, bool IsActive);

public sealed record CreateFieldRequest(
    int SportId,
    int LocationId,
    string Name,
    string Description,
    string Address,
    string City,
    FieldType FieldType,
    decimal Latitude,
    decimal Longitude,
    decimal DayPricePerHour,
    decimal NightPricePerHour,
    IReadOnlyCollection<string>? ImageUrls = null,
    IReadOnlyCollection<string>? AmenityNames = null,
    IReadOnlyCollection<int>? FacilityIds = null);

public sealed record UpdateFieldRequest(
    int SportId,
    int LocationId,
    string Name,
    string Description,
    string Address,
    string City,
    FieldType FieldType,
    decimal Latitude,
    decimal Longitude,
    decimal DayPricePerHour,
    decimal NightPricePerHour,
    IReadOnlyCollection<string>? ImageUrls = null,
    IReadOnlyCollection<string>? AmenityNames = null,
    IReadOnlyCollection<int>? FacilityIds = null);

public sealed record FieldManagementDto(
    int Id,
    string Name,
    string City,
    string Address,
    SportType SportType,
    string SportName,
    FieldType FieldType,
    decimal DayPricePerHour,
    decimal NightPricePerHour,
    decimal AverageRating,
    int ReviewCount,
    bool IsActive,
    bool IsApproved,
    DateTime? ApprovedAtUtc,
    IReadOnlyCollection<string> Images,
    IReadOnlyCollection<FacilityDto> Facilities,
    IReadOnlyCollection<FieldAmenityDto> Amenities);

public sealed record SetAvailabilityRequest(DateOnly Date, TimeOnly OpenTime, TimeOnly CloseTime, bool IsClosed);

public sealed record UpdateAvailabilityRequest(TimeOnly OpenTime, TimeOnly CloseTime, bool IsClosed);

public sealed record UpdateReviewRequest(int Rating, string? Comment);
