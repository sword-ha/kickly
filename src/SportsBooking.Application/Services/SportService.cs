using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class SportService : ISportService
{
    private readonly ISportRepository _sportRepository;

    public SportService(ISportRepository sportRepository)
    {
        _sportRepository = sportRepository;
    }

    public async Task<IReadOnlyCollection<SportDto>> GetAllAsync(CancellationToken ct = default)
    {
        var sports = await _sportRepository.GetAllAsync(ct);
        return sports
            .Where(s => s.IsActive)
            .Select(s => new SportDto(s.Id, s.Type, s.Name, s.Slug, s.Description))
            .ToList();
    }

    public async Task<SportDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var sport = await _sportRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Sport was not found.");
        return new SportDto(sport.Id, sport.Type, sport.Name, sport.Slug, sport.Description);
    }

    public async Task<SportDto> CreateAsync(CreateSportRequest request, CancellationToken ct = default)
    {
        if (await _sportRepository.SlugExistsAsync(request.Slug, null, ct))
        {
            throw new ConflictException("A sport with this slug already exists.");
        }

        var sport = new Sport
        {
            Type = request.Type,
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim().ToLowerInvariant(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = request.IsActive
        };

        await _sportRepository.AddAsync(sport, ct);
        await _sportRepository.SaveChangesAsync(ct);
        return new SportDto(sport.Id, sport.Type, sport.Name, sport.Slug, sport.Description);
    }

    public async Task<SportDto> UpdateAsync(int id, UpdateSportRequest request, CancellationToken ct = default)
    {
        var sport = await _sportRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Sport was not found.");

        if (await _sportRepository.SlugExistsAsync(request.Slug, id, ct))
        {
            throw new ConflictException("A sport with this slug already exists.");
        }

        sport.Type = request.Type;
        sport.Name = request.Name.Trim();
        sport.Slug = request.Slug.Trim().ToLowerInvariant();
        sport.Description = request.Description?.Trim() ?? string.Empty;
        sport.IsActive = request.IsActive;
        sport.UpdatedAtUtc = DateTime.UtcNow;

        await _sportRepository.SaveChangesAsync(ct);
        return new SportDto(sport.Id, sport.Type, sport.Name, sport.Slug, sport.Description);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var sport = await _sportRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Sport was not found.");

        _sportRepository.Remove(sport);
        await _sportRepository.SaveChangesAsync(ct);
    }
}
