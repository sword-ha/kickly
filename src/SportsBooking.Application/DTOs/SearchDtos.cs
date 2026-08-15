using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record SearchFieldsQuery(
    int? SportId = null,
    SportType? SportType = null,
    FieldType? FieldType = null,
    string? City = null,
    decimal? MinPricePerHour = null,
    decimal? MaxPricePerHour = null,
    decimal? MinRating = null,
    DateOnly? Date = null,
    IReadOnlyCollection<int>? FacilityIds = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    double? RadiusKm = null,
    SortBy SortBy = SortBy.Rating,
    int Page = 1,
    int PageSize = 20);

public sealed record NearbyFieldsQuery(
    decimal Latitude,
    decimal Longitude,
    double? RadiusKm = null,
    int? SportId = null,
    int Page = 1,
    int PageSize = 20);

public sealed record NearbyLocationsQuery(
    decimal Latitude,
    decimal Longitude,
    double? RadiusKm = null,
    int PageSize = 20);