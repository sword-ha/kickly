using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IReviewRepository _reviewRepository;

    public AdminDashboardService(
        IUserRepository userRepository,
        IFieldRepository fieldRepository,
        IBookingRepository bookingRepository,
        IReviewRepository reviewRepository)
    {
        _userRepository = userRepository;
        _fieldRepository = fieldRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<AdminDashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        var (bookings, _) = await _bookingRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var totalReviews = 0;

        foreach (var field in fields)
        {
            var fieldReviews = await _reviewRepository.GetFieldReviewsAsync(field.Id, ct);
            totalReviews += fieldReviews.Count;
        }

        return new AdminDashboardStatsDto(
            users.Count,
            users.Count(u => u.Role == UserRole.Owner),
            users.Count(u => u.Role == UserRole.Customer),
            fields.Count,
            fields.Count(f => !f.IsApproved),
            bookings.Count,
            bookings.Where(IsPaid).Sum(b => b.TotalPrice),
            totalReviews);
    }

    private static bool IsPaid(Domain.Entities.Booking b)
        => b.Status is BookingStatus.Confirmed or BookingStatus.Completed;
}
