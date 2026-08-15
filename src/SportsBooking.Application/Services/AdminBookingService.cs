using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class AdminBookingService : IAdminBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAuditLogService _auditLogService;

    public AdminBookingService(IBookingRepository bookingRepository, IAuditLogService auditLogService)
    {
        _bookingRepository = bookingRepository;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<AdminBookingDto>> GetBookingsAsync(int page, int pageSize, BookingStatus? status, CancellationToken ct = default)
    {
        var (items, total) = await _bookingRepository.GetPagedAsync(page, pageSize, status, ct);
        return new PagedResult<AdminBookingDto>(items.Select(Map).ToList(), total, page, pageSize);
    }

    public async Task<AdminBookingDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Booking was not found.");
        return Map(booking);
    }

    public async Task<AdminBookingDto> CancelAsync(int id, string? reason, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
        {
            throw new ValidationDomainException("This booking cannot be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;
        booking.CancellationReason = reason?.Trim();
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(null, "CancelBooking", nameof(Domain.Entities.Booking), id.ToString(), reason, ct);
        return Map(booking);
    }

    private static AdminBookingDto Map(Domain.Entities.Booking b)
        => new(
            b.Id,
            b.FieldId,
            b.Field.Name,
            b.UserId,
            $"{b.User.FirstName} {b.User.LastName}".Trim(),
            b.User.Email ?? string.Empty,
            b.BookingDate,
            b.StartTime,
            b.EndTime,
            b.DurationHours,
            b.TotalPrice,
            b.Status,
            b.CreatedAtUtc);
}
