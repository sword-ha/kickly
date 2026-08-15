using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record SportDto(int Id, SportType Type, string Name, string Slug, string Description);

public sealed record FieldImageDto(int Id, string ImageUrl, int DisplayOrder, bool IsPrimary);

public sealed record FieldAmenityDto(int Id, string Name, string Icon);

public sealed record FieldListItemDto(
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
    double DistanceKm,
    string? PrimaryImageUrl);

public sealed record FieldDetailsDto(
    int Id,
    string Name,
    string Description,
    string City,
    string Address,
    decimal Latitude,
    decimal Longitude,
    SportType SportType,
    string SportName,
    FieldType FieldType,
    decimal DayPricePerHour,
    decimal NightPricePerHour,
    decimal AverageRating,
    int ReviewCount,
    bool IsActive,
    IReadOnlyCollection<FieldImageDto> Images,
    IReadOnlyCollection<FieldAmenityDto> Amenities);

public sealed record AvailabilitySlotDto(TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable, int MaxConsecutiveHours);

public sealed record FieldAvailabilityDto(DateOnly Date, bool IsClosed, IReadOnlyCollection<AvailabilitySlotDto> Slots);

public sealed record ReviewDto(int Id, int BookingId, int UserId, string UserName, int FieldId, int Rating, string? Comment, DateTime CreatedAtUtc);

public sealed record FavoriteDto(int Id, int FieldId, string FieldName, string City, decimal DayPricePerHour, decimal NightPricePerHour, decimal AverageRating, string? PrimaryImageUrl);