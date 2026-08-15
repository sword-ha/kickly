using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class OwnerFieldService : IOwnerFieldService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly ISportRepository _sportRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IFacilityRepository _facilityRepository;
    private readonly IFieldAvailabilityRepository _availabilityRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldService _fieldService;
    private readonly INotificationService _notificationService;

    public OwnerFieldService(
        IFieldRepository fieldRepository,
        ISportRepository sportRepository,
        ILocationRepository locationRepository,
        IFacilityRepository facilityRepository,
        IFieldAvailabilityRepository availabilityRepository,
        IBookingRepository bookingRepository,
        IFieldService fieldService,
        INotificationService notificationService)
    {
        _fieldRepository = fieldRepository;
        _sportRepository = sportRepository;
        _locationRepository = locationRepository;
        _facilityRepository = facilityRepository;
        _availabilityRepository = availabilityRepository;
        _bookingRepository = bookingRepository;
        _fieldService = fieldService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyCollection<FieldManagementDto>> GetMyFieldsAsync(int ownerId, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetOwnerFieldsAsync(ownerId, ct);
        return fields.Select(MapManagement).ToList();
    }

    public async Task<FieldManagementDto> GetByIdAsync(int ownerId, int fieldId, CancellationToken ct = default)
    {
        var field = await GetOwnedFieldAsync(ownerId, fieldId, ct);
        return MapManagement(field);
    }

    public async Task<FieldManagementDto> CreateAsync(int ownerId, CreateFieldRequest request, CancellationToken ct = default)
    {
        await EnsureSportAndLocationAsync(request.SportId, request.LocationId, ct);
        var facilities = await ValidateFacilitiesAsync(request.FacilityIds, ct);

        var field = new Field
        {
            OwnerId = ownerId,
            SportId = request.SportId,
            LocationId = request.LocationId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            FieldType = request.FieldType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            DayPricePerHour = request.DayPricePerHour,
            NightPricePerHour = request.NightPricePerHour,
            IsActive = true,
            IsApproved = false,
            Images = BuildImages(request.ImageUrls).ToList(),
            Amenities = BuildAmenities(request.AmenityNames).ToList(),
            Facilities = facilities.ToList()
        };

        await _fieldRepository.AddAsync(field, ct);
        await _fieldRepository.SaveChangesAsync(ct);
        return MapManagement(field);
    }

    public async Task<FieldManagementDto> UpdateAsync(int ownerId, int fieldId, UpdateFieldRequest request, CancellationToken ct = default)
    {
        var field = await GetOwnedFieldAsync(ownerId, fieldId, ct);
        await EnsureSportAndLocationAsync(request.SportId, request.LocationId, ct);
        var facilities = await ValidateFacilitiesAsync(request.FacilityIds, ct);

        field.SportId = request.SportId;
        field.LocationId = request.LocationId;
        field.Name = request.Name.Trim();
        field.Description = request.Description?.Trim() ?? string.Empty;
        field.Address = request.Address.Trim();
        field.City = request.City.Trim();
        field.FieldType = request.FieldType;
        field.Latitude = request.Latitude;
        field.Longitude = request.Longitude;
        field.DayPricePerHour = request.DayPricePerHour;
        field.NightPricePerHour = request.NightPricePerHour;
        field.UpdatedAtUtc = DateTime.UtcNow;

        ReplaceImages(field, request.ImageUrls);
        ReplaceAmenities(field, request.AmenityNames);
        ReplaceFacilities(field, facilities);

        await _fieldRepository.SaveChangesAsync(ct);
        return MapManagement(field);
    }

    public async Task DeleteAsync(int ownerId, int fieldId, CancellationToken ct = default)
    {
        var field = await GetOwnedFieldAsync(ownerId, fieldId, ct);

        var bookings = await _bookingRepository.GetFieldBookingsAsync(fieldId, ct);
        if (bookings.Any(b => BookingStatusExtensions.OccupyingStatuses.Contains(b.Status)))
        {
            throw new ConflictException("This field has active bookings and cannot be deleted. Deactivate it instead.");
        }

        _fieldRepository.Remove(field);
        await _fieldRepository.SaveChangesAsync(ct);
    }

    public async Task<FieldAvailabilityDto> SetAvailabilityAsync(int ownerId, int fieldId, SetAvailabilityRequest request, CancellationToken ct = default)
    {
        await GetOwnedFieldAsync(ownerId, fieldId, ct);

        if (request.CloseTime <= request.OpenTime && !request.IsClosed)
        {
            throw new ValidationDomainException("Close time must be after open time.");
        }

        var existing = await _availabilityRepository.GetByFieldAndDateAsync(fieldId, request.Date, ct);
        if (existing is not null)
        {
            existing.OpenTime = request.OpenTime;
            existing.CloseTime = request.CloseTime;
            existing.IsClosed = request.IsClosed;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            await _availabilityRepository.AddAsync(new FieldAvailability
            {
                FieldId = fieldId,
                Date = request.Date,
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,
                IsClosed = request.IsClosed
            }, ct);
        }

        await _availabilityRepository.SaveChangesAsync(ct);
        return await _fieldService.GetAvailabilityAsync(fieldId, request.Date, ct);
    }

    public async Task<FieldAvailabilityDto> UpdateAvailabilityAsync(int ownerId, int fieldId, int availabilityId, UpdateAvailabilityRequest request, CancellationToken ct = default)
    {
        await GetOwnedFieldAsync(ownerId, fieldId, ct);

        if (request.CloseTime <= request.OpenTime && !request.IsClosed)
        {
            throw new ValidationDomainException("Close time must be after open time.");
        }

        var availability = await _availabilityRepository.GetByIdAsync(availabilityId, ct)
            ?? throw new NotFoundException("Availability was not found.");

        if (availability.FieldId != fieldId)
        {
            throw new ForbiddenException("This availability does not belong to your field.");
        }

        availability.OpenTime = request.OpenTime;
        availability.CloseTime = request.CloseTime;
        availability.IsClosed = request.IsClosed;
        availability.UpdatedAtUtc = DateTime.UtcNow;

        await _availabilityRepository.SaveChangesAsync(ct);
        return await _fieldService.GetAvailabilityAsync(fieldId, availability.Date, ct);
    }

    public async Task DeleteAvailabilityAsync(int ownerId, int fieldId, int availabilityId, CancellationToken ct = default)
    {
        await GetOwnedFieldAsync(ownerId, fieldId, ct);

        var availability = await _availabilityRepository.GetByIdAsync(availabilityId, ct)
            ?? throw new NotFoundException("Availability was not found.");

        if (availability.FieldId != fieldId)
        {
            throw new ForbiddenException("This availability does not belong to your field.");
        }

        _availabilityRepository.Remove(availability);
        await _availabilityRepository.SaveChangesAsync(ct);
    }

    private async Task<Field> GetOwnedFieldAsync(int ownerId, int fieldId, CancellationToken ct)
    {
        var field = await _fieldRepository.GetByIdWithFacilitiesAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        if (field.OwnerId != ownerId)
        {
            throw new ForbiddenException("You are not the owner of this field.");
        }

        return field;
    }

    private async Task EnsureSportAndLocationAsync(int sportId, int locationId, CancellationToken ct)
    {
        if (await _sportRepository.GetByIdAsync(sportId, ct) is null)
        {
            throw new NotFoundException("Sport was not found.");
        }

        if (await _locationRepository.GetByIdAsync(locationId, ct) is null)
        {
            throw new NotFoundException("Location was not found.");
        }
    }

    private async Task<IReadOnlyCollection<FieldFacility>> ValidateFacilitiesAsync(IReadOnlyCollection<int>? facilityIds, CancellationToken ct)
    {
        if (facilityIds is null || facilityIds.Count == 0)
        {
            return Array.Empty<FieldFacility>();
        }

        var distinct = facilityIds.Distinct().ToList();
        var facilities = await _facilityRepository.GetByIdsAsync(distinct, ct);
        if (facilities.Count != distinct.Count)
        {
            throw new NotFoundException("One or more facilities were not found.");
        }

        return distinct.Select(id => new FieldFacility { FacilityId = id }).ToList();
    }

    private static IReadOnlyCollection<FieldImage> BuildImages(IReadOnlyCollection<string>? urls)
    {
        if (urls is null || urls.Count == 0)
        {
            return Array.Empty<FieldImage>();
        }

        return urls
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select((url, index) => new FieldImage
            {
                ImageUrl = url.Trim(),
                DisplayOrder = index + 1,
                IsPrimary = index == 0
            })
            .ToList();
    }

    private static IReadOnlyCollection<FieldAmenity> BuildAmenities(IReadOnlyCollection<string>? names)
    {
        if (names is null || names.Count == 0)
        {
            return Array.Empty<FieldAmenity>();
        }

        return names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => new FieldAmenity { Name = n.Trim() })
            .ToList();
    }

    private static void ReplaceImages(Field field, IReadOnlyCollection<string>? urls)
    {
        field.Images.Clear();
        foreach (var image in BuildImages(urls))
        {
            field.Images.Add(image);
        }
    }

    private static void ReplaceAmenities(Field field, IReadOnlyCollection<string>? names)
    {
        field.Amenities.Clear();
        foreach (var amenity in BuildAmenities(names))
        {
            field.Amenities.Add(amenity);
        }
    }

    private static void ReplaceFacilities(Field field, IReadOnlyCollection<FieldFacility> facilities)
    {
        field.Facilities.Clear();
        foreach (var facility in facilities)
        {
            field.Facilities.Add(facility);
        }
    }

    private static FieldManagementDto MapManagement(Field f)
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
            f.IsActive,
            f.IsApproved,
            f.ApprovedAtUtc,
            f.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
            f.Facilities.OrderBy(x => x.Facility.Name).Select(x => new FacilityDto(x.Facility.Id, x.Facility.Name, x.Facility.Icon, x.Facility.IsActive)).ToList(),
            f.Amenities.OrderBy(a => a.Name).Select(a => new FieldAmenityDto(a.Id, a.Name, a.Icon)).ToList());
}
