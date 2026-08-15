using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class FacilityService : IFacilityService
{
    private readonly IFacilityRepository _facilityRepository;

    public FacilityService(IFacilityRepository facilityRepository)
    {
        _facilityRepository = facilityRepository;
    }

    public async Task<IReadOnlyCollection<FacilityDto>> GetAllAsync(CancellationToken ct = default)
    {
        var facilities = await _facilityRepository.GetAllAsync(ct);
        return facilities
            .OrderBy(f => f.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<IReadOnlyCollection<FacilityDto>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var facilities = await _facilityRepository.GetAllAsync(ct);
        return facilities
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<FacilityDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var facility = await _facilityRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Facility was not found.");
        return Map(facility);
    }

    public async Task<FacilityDto> CreateAsync(CreateFacilityRequest request, CancellationToken ct = default)
    {
        var facility = new Facility
        {
            Name = request.Name.Trim(),
            Icon = request.Icon?.Trim() ?? string.Empty,
            IsActive = true
        };

        await _facilityRepository.AddAsync(facility, ct);
        await _facilityRepository.SaveChangesAsync(ct);
        return Map(facility);
    }

    public async Task<FacilityDto> UpdateAsync(int id, UpdateFacilityRequest request, CancellationToken ct = default)
    {
        var facility = await _facilityRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Facility was not found.");

        facility.Name = request.Name.Trim();
        facility.Icon = request.Icon?.Trim() ?? string.Empty;
        facility.IsActive = request.IsActive;
        facility.UpdatedAtUtc = DateTime.UtcNow;

        await _facilityRepository.SaveChangesAsync(ct);
        return Map(facility);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var facility = await _facilityRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Facility was not found.");

        if (await _facilityRepository.IsInUseAsync(id, ct))
        {
            // Facilities that are still assigned to fields are deactivated instead of deleted.
            facility.IsActive = false;
            facility.UpdatedAtUtc = DateTime.UtcNow;
            await _facilityRepository.SaveChangesAsync(ct);
            return;
        }

        _facilityRepository.Remove(facility);
        await _facilityRepository.SaveChangesAsync(ct);
    }

    private static FacilityDto Map(Facility facility)
        => new(facility.Id, facility.Name, facility.Icon, facility.IsActive);
}
