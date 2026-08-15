using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.Services;

public sealed class AdminReportService : IAdminReportService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IUserRepository _userRepository;

    public AdminReportService(
        IBookingRepository bookingRepository,
        IFieldRepository fieldRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _fieldRepository = fieldRepository;
        _userRepository = userRepository;
    }

    public async Task<AdminReportDto> GetReportAsync(CancellationToken ct = default)
    {
        var (bookings, _) = await _bookingRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);
        var users = await _userRepository.GetPagedAsync(1, int.MaxValue, null, ct);

        var recent = bookings
            .OrderByDescending(b => b.CreatedAtUtc)
            .Take(10)
            .Select(b => new BookingDto(
                b.Id, b.FieldId, b.Field.Name, b.Field.City, b.BookingDate,
                b.StartTime, b.EndTime, b.DurationHours, b.TotalPrice, b.Status, b.CreatedAtUtc))
            .ToList();

        var topFields = fields
            .OrderByDescending(f => f.ReviewCount)
            .ThenByDescending(f => f.AverageRating)
            .Take(5)
            .Select(f => new AdminFieldDto(
                f.Id, f.Name, f.City, f.Sport.Type, f.Sport.Name,
                f.Owner is not null ? $"{f.Owner.FirstName} {f.Owner.LastName}".Trim() : null,
                f.DayPricePerHour, f.NightPricePerHour, f.AverageRating, f.IsActive, f.IsApproved, f.CreatedAtUtc))
            .ToList();

        var activeUsers = users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Role)
            .ThenBy(u => u.CreatedAtUtc)
            .Take(5)
            .Select(u => new AdminUserDto(u.Id, u.FirstName, u.LastName, u.Email ?? string.Empty, u.PhoneNumber ?? string.Empty, u.Role, u.IsActive, u.CreatedAtUtc))
            .ToList();

        return new AdminReportDto(
            bookings.Count,
            bookings.Count(b => b.Status == BookingStatus.Completed),
            bookings.Count(b => b.Status == BookingStatus.Cancelled),
            bookings.Count(b => b.Status == BookingStatus.PendingPayment),
            bookings.Count(b => b.Status == BookingStatus.Confirmed),
            bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Completed).Sum(b => b.TotalPrice),
            0m,
            users.Count,
            fields.Count,
            recent,
            topFields,
            activeUsers);
    }
}
