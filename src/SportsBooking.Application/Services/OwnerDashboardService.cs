using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.Services;

public sealed class OwnerDashboardService : IOwnerDashboardService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IBookingRepository _bookingRepository;

    public OwnerDashboardService(
        IFieldRepository fieldRepository,
        IBookingRepository bookingRepository)
    {
        _fieldRepository = fieldRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<OwnerDashboardStatsDto> GetStatsAsync(int ownerId, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetOwnerFieldsAsync(ownerId, ct);
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        var (bookings, _) = await _bookingRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var ownerBookings = bookings.Where(b => fieldIds.Contains(b.FieldId)).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new OwnerDashboardStatsDto(
            fields.Count,
            fields.Count(f => f.IsActive),
            fields.Count(f => !f.IsApproved),
            ownerBookings.Count,
            ownerBookings.Count(b => b.BookingDate >= today && BookingStatusExtensions.OccupyingStatuses.Contains(b.Status)),
            ownerBookings.Where(IsPaid).Sum(b => b.TotalPrice));
    }

    public async Task<OwnerRevenueDto> GetRevenueAsync(int ownerId, int days, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetOwnerFieldsAsync(ownerId, ct);
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        var (bookings, _) = await _bookingRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var paid = bookings
            .Where(b => fieldIds.Contains(b.FieldId) && IsPaid(b))
            .ToList();

        var totalRevenue = paid.Sum(b => b.TotalPrice);
        var monthStart = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(DateOnly.FromDateTime(DateTime.UtcNow).Day - 1));
        var monthRevenue = paid.Where(b => b.BookingDate >= monthStart).Sum(b => b.TotalPrice);

        days = Math.Clamp(days, 1, 365);
        var daily = new List<OwnerRevenuePointDto>();
        var start = DateTime.UtcNow.Date.AddDays(-(days - 1));

        for (var i = 0; i < days; i++)
        {
            var day = start.AddDays(i);
            var dayDate = DateOnly.FromDateTime(day);
            var revenue = paid.Where(b => b.BookingDate == dayDate).Sum(b => b.TotalPrice);
            daily.Add(new OwnerRevenuePointDto(day, revenue));
        }

        return new OwnerRevenueDto(totalRevenue, monthRevenue, daily);
    }

    private static bool IsPaid(Domain.Entities.Booking b)
        => b.Status is BookingStatus.Confirmed or BookingStatus.Completed;

    public async Task<IReadOnlyCollection<OwnerFieldPerformanceDto>> GetFieldPerformanceAsync(int ownerId, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetOwnerFieldsAsync(ownerId, ct);
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        var (bookings, _) = await _bookingRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var ownerBookings = bookings.Where(b => fieldIds.Contains(b.FieldId)).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return fields
            .Select(f => new OwnerFieldPerformanceDto(
                f.Id,
                f.Name,
                ownerBookings.Count(b => b.FieldId == f.Id),
                ownerBookings.Count(b => b.FieldId == f.Id && b.BookingDate >= today && BookingStatusExtensions.OccupyingStatuses.Contains(b.Status)),
                ownerBookings.Where(b => b.FieldId == f.Id && IsPaid(b)).Sum(b => b.TotalPrice),
                f.AverageRating,
                f.ReviewCount,
                f.IsActive))
            .OrderByDescending(p => p.Revenue)
            .ToList();
    }

    public async Task<IReadOnlyCollection<OwnerBookingDto>> GetUpcomingBookingsAsync(int ownerId, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetOwnerFieldsAsync(ownerId, ct);
        var fieldIds = fields.Select(f => f.Id).ToHashSet();

        var (bookings, _) = await _bookingRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return bookings
            .Where(b => fieldIds.Contains(b.FieldId))
            .Where(b => b.BookingDate >= today && BookingStatusExtensions.OccupyingStatuses.Contains(b.Status))
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .Take(50)
            .Select(b => new OwnerBookingDto(
                b.Id,
                b.FieldId,
                b.Field.Name,
                $"{b.User.FirstName} {b.User.LastName}".Trim(),
                b.User.Email ?? string.Empty,
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.DurationHours,
                b.TotalPrice,
                b.Status,
                b.CreatedAtUtc))
            .ToList();
    }
}
