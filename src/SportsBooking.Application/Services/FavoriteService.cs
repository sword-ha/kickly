using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IFieldRepository _fieldRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository, IFieldRepository fieldRepository)
    {
        _favoriteRepository = favoriteRepository;
        _fieldRepository = fieldRepository;
    }

    public async Task AddAsync(int userId, int fieldId, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        if (await _favoriteRepository.ExistsAsync(userId, fieldId, ct))
        {
            throw new ConflictException("Field is already in your favorites.");
        }

        await _favoriteRepository.AddAsync(new Favorite { UserId = userId, FieldId = fieldId }, ct);
        await _favoriteRepository.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int userId, int fieldId, CancellationToken ct = default)
    {
        var favorite = await _favoriteRepository.GetAsync(userId, fieldId, ct)
            ?? throw new NotFoundException("Favorite was not found.");

        _favoriteRepository.Remove(favorite);
        await _favoriteRepository.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(int userId, int fieldId, CancellationToken ct = default)
        => await _favoriteRepository.ExistsAsync(userId, fieldId, ct);

    public async Task<int> CountAsync(int userId, CancellationToken ct = default)
        => await _favoriteRepository.CountAsync(userId, ct);

    public async Task<IReadOnlyCollection<FavoriteDto>> GetAsync(int userId, CancellationToken ct = default)
    {
        var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId, ct);
        return favorites.Select(f => new FavoriteDto(
                f.Id,
                f.FieldId,
                f.Field.Name,
                f.Field.City,
                f.Field.DayPricePerHour,
                f.Field.NightPricePerHour,
                f.Field.AverageRating,
                f.Field.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl))
            .ToList();
    }
}