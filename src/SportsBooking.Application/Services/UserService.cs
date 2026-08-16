using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IReviewRepository _reviewRepository;

    public UserService(
        IUserRepository userRepository,
        IBookingRepository bookingRepository,
        IFavoriteRepository favoriteRepository,
        IReviewRepository reviewRepository)
    {
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _favoriteRepository = favoriteRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User was not found.");
        return Map(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User was not found.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(ct);
        return Map(user);
    }

    public async Task<UserProfileDto> UpdateLocationAsync(int userId, UpdateLocationRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User was not found.");

        user.Latitude = request.Latitude;
        user.Longitude = request.Longitude;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(ct);
        return Map(user);
    }

    public async Task<UserStatsDto> GetStatsAsync(int userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId, ct);
        var favoritesCount = await _favoriteRepository.CountAsync(userId, ct);
        var reviews = await _reviewRepository.GetByUserAsync(userId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new UserStatsDto(
            bookings.Count,
            bookings.Count(b => b.BookingDate >= today && BookingStatusExtensions.OccupyingStatuses.Contains(b.Status)),
            bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Completed).Sum(b => b.TotalPrice),
            favoritesCount,
            reviews.Count);
    }

    public async Task DeactivateAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User was not found.");

        if (user.Role == UserRole.Admin)
        {
            throw new ValidationDomainException("Admin accounts cannot be deactivated.");
        }

        if (!user.IsActive)
        {
            return;
        }

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(ct);
    }

    private static UserProfileDto Map(Domain.Entities.User user)
        => new(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.PhoneNumber ?? string.Empty, user.Role, user.Latitude, user.Longitude);
}