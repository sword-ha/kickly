using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

    private static UserProfileDto Map(Domain.Entities.User user)
        => new(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.PhoneNumber ?? string.Empty, user.Role, user.Latitude, user.Longitude);
}