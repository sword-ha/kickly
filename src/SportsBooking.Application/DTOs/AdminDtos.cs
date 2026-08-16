using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record AdminDashboardStatsDto(
    int TotalUsers,
    int TotalOwners,
    int TotalCustomers,
    int TotalFields,
    int PendingFields,
    int TotalBookings,
    decimal TotalRevenue,
    int TotalReviews);

public sealed record AdminUserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record UpdateUserStatusRequest(bool IsActive);

public sealed record AdminFieldDto(
    int Id,
    string Name,
    string City,
    SportType SportType,
    string SportName,
    string? OwnerName,
    decimal DayPricePerHour,
    decimal NightPricePerHour,
    decimal AverageRating,
    bool IsActive,
    bool IsApproved,
    DateTime CreatedAtUtc);

public sealed record SetFieldApprovalRequest(bool IsApproved, string? Reason);

public sealed record AdminBookingDto(
    int Id,
    int FieldId,
    string FieldName,
    int UserId,
    string CustomerName,
    string CustomerEmail,
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationHours,
    decimal TotalPrice,
    BookingStatus Status,
    DateTime CreatedAtUtc);

public sealed record AdminReportDto(
    int TotalBookings,
    int CompletedBookings,
    int CancelledBookings,
    int PendingPayments,
    int ConfirmedBookings,
    decimal TotalRevenue,
    decimal RefundedAmount,
    int TotalUsers,
    int TotalFields,
    IReadOnlyCollection<BookingDto> RecentBookings,
    IReadOnlyCollection<AdminFieldDto> TopFields,
    IReadOnlyCollection<AdminUserDto> ActiveUsers);

public sealed record AuditLogDto(int Id, int? UserId, string Action, string EntityName, string EntityId, string? Details, DateTime CreatedAtUtc);

public sealed record AdminPaymentDto(
    int Id,
    int BookingId,
    int FieldId,
    string FieldName,
    int UserId,
    string CustomerName,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    string? Provider,
    string? TransactionId,
    string? FailureReason,
    DateTime? PaidAtUtc,
    DateTime? RefundedAtUtc,
    DateTime CreatedAtUtc);

public sealed record AdminReviewDto(
    int Id,
    int BookingId,
    int FieldId,
    string FieldName,
    int UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAtUtc);

public sealed record AdminTrendPointDto(DateTime Date, int BookingsCount, int CancelledCount, decimal Revenue);

public sealed record AdminTrendsDto(IReadOnlyCollection<AdminTrendPointDto> Daily);
