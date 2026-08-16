using Microsoft.Extensions.Options;
using SportsBooking.Application.Common;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly LocationOptions _locationOptions;

    public LocationService(
        ILocationRepository locationRepository,
        IFieldRepository fieldRepository,
        IAuditLogService auditLogService,
        IOptions<LocationOptions> locationOptions)
    {
        _locationRepository = locationRepository;
        _fieldRepository = fieldRepository;
        _auditLogService = auditLogService;
        _locationOptions = locationOptions.Value;
    }

    public async Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var locations = await _locationRepository.GetAllAsync(ct);
        return locations
            .OrderBy(l => l.Governorate)
            .ThenBy(l => l.Name)
            .Select(l => new LocationDto(l.Id, l.Name, l.City, l.Governorate, l.Address, l.Latitude, l.Longitude))
            .ToList();
    }

    public async Task<LocationDetailsDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Location was not found.");

        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        var count = fields.Count(f => f.LocationId == id && f.IsActive && f.IsApproved);

        return new LocationDetailsDto(
            location.Id, location.Name, location.City, location.Governorate,
            location.Address, location.Latitude, location.Longitude, count);
    }

    public async Task<IReadOnlyCollection<LocationDto>> GetNearbyAsync(NearbyLocationsQuery query, CancellationToken ct = default)
    {
        var radius = query.RadiusKm ?? _locationOptions.DefaultRadiusKm;
        var locations = await _locationRepository.GetAllAsync(ct);

        return locations
            .Where(l => GeoCalculator.IsWithinRadiusKm(
                (double)query.Latitude, (double)query.Longitude,
                (double)l.Latitude, (double)l.Longitude, radius))
            .OrderBy(l => GeoCalculator.DistanceKm(
                (double)query.Latitude, (double)query.Longitude,
                (double)l.Latitude, (double)l.Longitude))
            .Take(Math.Clamp(query.PageSize, 1, 100))
            .Select(l => new LocationDto(l.Id, l.Name, l.City, l.Governorate, l.Address, l.Latitude, l.Longitude))
            .ToList();
    }

    public async Task<PagedResult<LocationDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _locationRepository.GetPagedAsync(page, pageSize, ct);

        return new PagedResult<LocationDto>(
            items.Select(l => new LocationDto(l.Id, l.Name, l.City, l.Governorate, l.Address, l.Latitude, l.Longitude)).ToList(),
            total,
            page,
            pageSize);
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequest request, CancellationToken ct = default)
    {
        var location = new Location
        {
            Name = request.Name.Trim(),
            City = request.City.Trim(),
            Governorate = request.Governorate.Trim(),
            Address = request.Address?.Trim() ?? string.Empty,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        await _locationRepository.AddAsync(location, ct);
        await _locationRepository.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            null, "Create", nameof(Location), location.Id.ToString(), $"\"{location.Name}\"", ct);

        return ToDto(location);
    }

    public async Task<LocationDto> UpdateAsync(int id, AdminUpdateLocationRequest request, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Location was not found.");

        location.Name = request.Name.Trim();
        location.City = request.City.Trim();
        location.Governorate = request.Governorate.Trim();
        location.Address = request.Address?.Trim() ?? string.Empty;
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.UpdatedAtUtc = DateTime.UtcNow;

        _locationRepository.Update(location);
        await _locationRepository.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            null, "Update", nameof(Location), location.Id.ToString(), $"\"{location.Name}\"", ct);

        return ToDto(location);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Location was not found.");

        if (await _locationRepository.IsInUseAsync(id, ct))
        {
            throw new ConflictException("Location cannot be deleted because it is assigned to fields.");
        }

        _locationRepository.Remove(location);
        await _locationRepository.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            null, "Delete", nameof(Location), id.ToString(), $"\"{location.Name}\"", ct);
    }

    private static LocationDto ToDto(Location location)
        => new(location.Id, location.Name, location.City, location.Governorate, location.Address, location.Latitude, location.Longitude);
}
