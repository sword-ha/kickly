using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record CreateBookingRequest(int FieldId, DateOnly Date, TimeOnly StartTime, int DurationHours);

public sealed record CancelBookingRequest(string? Reason);

public sealed record BookingDto(
    int Id,
    int FieldId,
    string FieldName,
    string City,
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationHours,
    decimal TotalPrice,
    BookingStatus Status,
    DateTime CreatedAtUtc);

public sealed record BookingPreviewDto(
    int FieldId,
    string FieldName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationHours,
    bool IsAvailable,
    decimal TotalPrice,
    IReadOnlyCollection<PriceLineDto> PriceLines,
    string? Message);

public sealed record PriceLineDto(TimeOnly FromTime, TimeOnly ToTime, DayPeriod Period, decimal HourlyRate, int Hours, decimal LineTotal);

public sealed record CreateReviewRequest(int BookingId, int Rating, string? Comment);

public sealed record BookingStatsDto(
    int TotalBookings,
    int UpcomingBookings,
    int PastBookings,
    int PendingPayment,
    int Confirmed,
    int Completed,
    int Cancelled,
    decimal TotalSpent);