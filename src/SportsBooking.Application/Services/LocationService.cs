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
    private readonly LocationOptions _locationOptions;

    public LocationService(
        ILocationRepository locationRepository,
        IFieldRepository fieldRepository,
        IOptions<LocationOptions> locationOptions)
    {
        _locationRepository = locationRepository;
        _fieldRepository = fieldRepository;
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
}
