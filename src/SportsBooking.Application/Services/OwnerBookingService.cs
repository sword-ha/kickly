using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class OwnerBookingService : IOwnerBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly INotificationService _notificationService;

    public OwnerBookingService(
        IBookingRepository bookingRepository,
        IFieldRepository fieldRepository,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _fieldRepository = fieldRepository;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyCollection<OwnerBookingDto>> GetFieldBookingsAsync(int ownerId, int fieldId, CancellationToken ct = default)
    {
        await EnsureOwnedAsync(ownerId, fieldId, ct);

        var bookings = await _bookingRepository.GetFieldBookingsAsync(fieldId, ct);
        return bookings
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .Select(Map)
            .ToList();
    }

    public async Task<OwnerBookingDto> GetByIdAsync(int ownerId, int bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.Field.OwnerId != ownerId)
        {
            throw new ForbiddenException("This booking does not belong to your field.");
        }

        return Map(booking);
    }

    public async Task<OwnerBookingDto> UpdateStatusAsync(int ownerId, int bookingId, UpdateOwnerBookingStatusRequest request, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.Field.OwnerId != ownerId)
        {
            throw new ForbiddenException("This booking does not belong to your field.");
        }

        switch (request.Status)
        {
            case BookingStatus.Cancelled:
                if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
                {
                    throw new ValidationDomainException("This booking cannot be cancelled.");
                }

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAtUtc = DateTime.UtcNow;
                booking.CancellationReason = request.Reason?.Trim();
                booking.UpdatedAtUtc = DateTime.UtcNow;
                break;

            case BookingStatus.Completed:
                if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
                {
                    throw new ValidationDomainException("This booking cannot be completed.");
                }

                booking.Status = BookingStatus.Completed;
                booking.CompletedAtUtc = DateTime.UtcNow;
                booking.UpdatedAtUtc = DateTime.UtcNow;
                break;

            default:
                throw new ValidationDomainException("Only cancellation or completion can be applied.");
        }

        await _bookingRepository.SaveChangesAsync(ct);
        return Map(booking);
    }

    private async Task EnsureOwnedAsync(int ownerId, int fieldId, CancellationToken ct)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        if (field.OwnerId != ownerId)
        {
            throw new ForbiddenException("You are not the owner of this field.");
        }
    }

    private static OwnerBookingDto Map(Booking b)
        => new(
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
            b.CreatedAtUtc);
}
