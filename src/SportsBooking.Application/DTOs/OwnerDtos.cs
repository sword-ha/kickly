using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record OwnerDashboardStatsDto(
    int TotalFields,
    int ActiveFields,
    int PendingFields,
    int TotalBookings,
    int UpcomingBookings,
    decimal TotalRevenue);

public sealed record OwnerRevenuePointDto(DateTime Date, decimal Revenue);

public sealed record OwnerRevenueDto(decimal TotalRevenue, decimal MonthRevenue, IReadOnlyCollection<OwnerRevenuePointDto> DailyRevenue);

public sealed record OwnerBookingDto(
    int Id,
    int FieldId,
    string FieldName,
    string CustomerName,
    string CustomerEmail,
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationHours,
    decimal TotalPrice,
    BookingStatus Status,
    DateTime CreatedAtUtc);

public sealed record UpdateOwnerBookingStatusRequest(BookingStatus Status, string? Reason);
