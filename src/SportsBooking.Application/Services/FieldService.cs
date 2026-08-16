using Microsoft.Extensions.Options;
using SportsBooking.Application.Common;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class FieldService : IFieldService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IFieldAvailabilityRepository _availabilityRepository;
    private readonly LocationOptions _locationOptions;

    public FieldService(
        IFieldRepository fieldRepository,
        IBookingRepository bookingRepository,
        IReviewRepository reviewRepository,
        IFieldAvailabilityRepository availabilityRepository,
        IOptions<LocationOptions> locationOptions)
    {
        _fieldRepository = fieldRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _availabilityRepository = availabilityRepository;
        _locationOptions = locationOptions.Value;
    }

    public async Task<IReadOnlyCollection<FieldListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        return fields.Where(f => f.IsActive && f.IsApproved).Select(f => MapListItem(f, 0)).ToList();
    }

    public async Task<FieldDetailsDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetFieldDetailsAsync(id, ct)
            ?? throw new NotFoundException("Field was not found.");
        return MapDetails(field);
    }

    public async Task<PagedResult<FieldListItemDto>> SearchAsync(SearchFieldsQuery query, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        var filtered = fields
            .Where(f => f.IsActive && f.IsApproved)
            .AsEnumerable();

        if (query.SportId.HasValue)
        {
            filtered = filtered.Where(f => f.SportId == query.SportId.Value);
        }

        if (query.SportType.HasValue)
        {
            filtered = filtered.Where(f => f.Sport.Type == query.SportType.Value);
        }

        if (query.FieldType.HasValue)
        {
            filtered = filtered.Where(f => f.FieldType == query.FieldType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            filtered = filtered.Where(f => f.City.Contains(query.City, StringComparison.OrdinalIgnoreCase));
        }

        if (query.MinPricePerHour.HasValue)
        {
            filtered = filtered.Where(f => f.DayPricePerHour >= query.MinPricePerHour.Value);
        }

        if (query.MaxPricePerHour.HasValue)
        {
            filtered = filtered.Where(f => f.DayPricePerHour <= query.MaxPricePerHour.Value);
        }

        if (query.MinRating.HasValue)
        {
            filtered = filtered.Where(f => f.AverageRating >= query.MinRating.Value);
        }

        if (query.FacilityIds is { Count: > 0 })
        {
            var ids = query.FacilityIds.ToHashSet();
            filtered = filtered.Where(f => f.Facilities.Any(x => ids.Contains(x.FacilityId)));
        }

        if (query.Date.HasValue)
        {
            filtered = filtered.Where(f => !f.Bookings.Any(b =>
                b.BookingDate == query.Date.Value &&
                BookingStatusExtensions.OccupyingStatuses.Contains(b.Status)));
        }

        double? originLat = null;
        double? originLon = null;
        var radius = query.RadiusKm ?? _locationOptions.DefaultRadiusKm;

        if (query.Latitude.HasValue && query.Longitude.HasValue)
        {
            originLat = (double)query.Latitude.Value;
            originLon = (double)query.Longitude.Value;
            filtered = filtered.Where(f =>
                GeoCalculator.IsWithinRadiusKm(originLat.Value, originLon.Value, (double)f.Latitude, (double)f.Longitude, radius));
        }

        var list = filtered
            .Select(f => MapListItem(f, originLat.HasValue ? GeoCalculator.DistanceKm(originLat.Value, originLon!.Value, (double)f.Latitude, (double)f.Longitude) : 0))
            .ToList();

        var sorted = query.SortBy switch
        {
            SortBy.Distance => list.OrderBy(x => x.DistanceKm),
            SortBy.Rating => list.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.ReviewCount),
            SortBy.PriceAsc => list.OrderBy(x => x.DayPricePerHour),
            SortBy.PriceDesc => list.OrderByDescending(x => x.DayPricePerHour),
            SortBy.ReviewCount => list.OrderByDescending(x => x.ReviewCount),
            _ => list.OrderByDescending(x => x.AverageRating)
        };

        var totalCount = sorted.Count();
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<FieldListItemDto>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyCollection<FieldListItemDto>> GetNearbyAsync(NearbyFieldsQuery query, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        var radius = query.RadiusKm ?? _locationOptions.DefaultRadiusKm;

        var nearby = fields
            .Where(f => f.IsActive && f.IsApproved)
            .Where(f => !query.SportId.HasValue || f.SportId == query.SportId.Value)
            .Where(f => GeoCalculator.IsWithinRadiusKm((double)query.Latitude, (double)query.Longitude, (double)f.Latitude, (double)f.Longitude, radius))
            .Select(f => MapListItem(f, GeoCalculator.DistanceKm((double)query.Latitude, (double)query.Longitude, (double)f.Latitude, (double)f.Longitude)))
            .OrderBy(x => x.DistanceKm)
            .Take(Math.Clamp(query.PageSize, 1, 100))
            .ToList();

        return nearby;
    }

    public async Task<IReadOnlyCollection<FieldListItemDto>> GetTopRatedAsync(double latitude, double longitude, double? radiusKm, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        var radius = radiusKm ?? _locationOptions.DefaultRadiusKm;

        // Filter by radius first, then sort by rating.
        return fields
            .Where(f => f.IsActive && f.IsApproved)
            .Where(f => GeoCalculator.IsWithinRadiusKm(latitude, longitude, (double)f.Latitude, (double)f.Longitude, radius))
            .Select(f => MapListItem(f, GeoCalculator.DistanceKm(latitude, longitude, (double)f.Latitude, (double)f.Longitude)))
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(10)
            .ToList();
    }

    public async Task<IReadOnlyCollection<FieldListItemDto>> GetFeaturedAsync(int count, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);

        return fields
            .Where(f => f.IsActive && f.IsApproved)
            .Select(f => MapListItem(f, 0))
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(Math.Clamp(count, 1, 50))
            .ToList();
    }

    public async Task<IReadOnlyCollection<FieldCityDto>> GetCitiesAsync(CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);

        return fields
            .Where(f => f.IsActive && f.IsApproved)
            .GroupBy(f => f.City)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Select(g => new FieldCityDto(g.Key!, g.Count()))
            .OrderBy(c => c.City)
            .ToList();
    }

    public async Task<IReadOnlyCollection<FieldListItemDto>> GetSimilarAsync(int fieldId, int count, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        var fields = await _fieldRepository.GetAllFieldsAsync(ct);

        return fields
            .Where(f => f.IsActive && f.IsApproved && f.Id != field.Id)
            .Select(f => MapListItem(f, 0))
            .OrderByDescending(x => x.SportType == field.Sport.Type)
            .ThenByDescending(x => x.City == field.City)
            .ThenByDescending(x => x.AverageRating)
            .Take(Math.Clamp(count, 1, 50))
            .ToList();
    }

    public async Task<IReadOnlyCollection<FieldAvailabilityDto>> GetScheduleAsync(int fieldId, DateOnly startDate, int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 90);
        var result = new List<FieldAvailabilityDto>();

        for (var i = 0; i < days; i++)
        {
            result.Add(await GetAvailabilityAsync(fieldId, startDate.AddDays(i), ct));
        }

        return result;
    }

    public async Task<FieldAvailabilityDto> GetAvailabilityAsync(int fieldId, DateOnly date, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        var availability = await _availabilityRepository.GetByFieldAndDateAsync(fieldId, date, ct);

        if (availability is not null && availability.IsClosed)
        {
            return new FieldAvailabilityDto(date, true, Array.Empty<AvailabilitySlotDto>());
        }

        var openTime = availability?.OpenTime ?? new TimeOnly(8, 0);
        var closeTime = availability?.CloseTime ?? new TimeOnly(23, 0);
        var bookings = await _bookingRepository.GetFieldBookingsByDateAsync(fieldId, date, ct);

        var slots = BuildSlots(openTime, closeTime, bookings);
        return new FieldAvailabilityDto(date, false, slots);
    }

    public async Task<IReadOnlyCollection<ReviewDto>> GetReviewsAsync(int fieldId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepository.GetFieldReviewsAsync(fieldId, ct);
        return reviews.Select(r => new ReviewDto(
                r.Id,
                r.BookingId,
                r.UserId,
                $"{r.User.FirstName} {r.User.LastName}".Trim(),
                r.FieldId,
                r.Rating,
                r.Comment,
                r.CreatedAtUtc))
            .ToList();
    }

    private static IReadOnlyCollection<AvailabilitySlotDto> BuildSlots(TimeOnly openTime, TimeOnly closeTime, IReadOnlyCollection<Booking> bookings)
    {
        var result = new List<AvailabilitySlotDto>();
        var current = openTime;

        while (current < closeTime)
        {
            var next = current.AddHours(1);
            if (next > closeTime)
            {
                break;
            }

            var isAvailable = !bookings.Any(b =>
                BookingStatusExtensions.OccupyingStatuses.Contains(b.Status) &&
                b.StartTime < next &&
                b.EndTime > current);

            var maxConsecutiveHours = 0;
            if (isAvailable)
            {
                var probe = current;
                while (probe < closeTime)
                {
                    var probeEnd = probe.AddHours(1);
                    if (probeEnd > closeTime || bookings.Any(b =>
                            BookingStatusExtensions.OccupyingStatuses.Contains(b.Status) &&
                            b.StartTime < probeEnd &&
                            b.EndTime > probe))
                    {
                        break;
                    }

                    maxConsecutiveHours++;
                    probe = probeEnd;
                }
            }

            result.Add(new AvailabilitySlotDto(current, next, isAvailable, maxConsecutiveHours));
            current = next;
        }

        return result;
    }

    internal static FieldListItemDto MapListItem(Field f, double distanceKm)
        => new(
            f.Id,
            f.Name,
            f.City,
            f.Address,
            f.Sport.Type,
            f.Sport.Name,
            f.FieldType,
            f.DayPricePerHour,
            f.NightPricePerHour,
            f.AverageRating,
            f.ReviewCount,
            distanceKm,
            f.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl);

    internal static FieldDetailsDto MapDetails(Field f)
        => new(
            f.Id,
            f.Name,
            f.Description,
            f.City,
            f.Address,
            f.Latitude,
            f.Longitude,
            f.Sport.Type,
            f.Sport.Name,
            f.FieldType,
            f.DayPricePerHour,
            f.NightPricePerHour,
            f.AverageRating,
            f.ReviewCount,
            f.IsActive,
            f.Images.OrderBy(i => i.DisplayOrder).Select(i => new FieldImageDto(i.Id, i.ImageUrl, i.DisplayOrder, i.IsPrimary)).ToList(),
            f.Amenities.Select(a => new FieldAmenityDto(a.Id, a.Name, a.Icon)).ToList());
}