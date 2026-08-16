using Microsoft.Extensions.Options;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFieldAvailabilityRepository _availabilityRepository;
    private readonly INotificationService _notificationService;
    private readonly BookingOptions _bookingOptions;
    private readonly PricingOptions _pricingOptions;

    public BookingService(
        IBookingRepository bookingRepository,
        IFieldRepository fieldRepository,
        IUserRepository userRepository,
        IFieldAvailabilityRepository availabilityRepository,
        INotificationService notificationService,
        IOptions<BookingOptions> bookingOptions,
        IOptions<PricingOptions> pricingOptions)
    {
        _bookingRepository = bookingRepository;
        _fieldRepository = fieldRepository;
        _userRepository = userRepository;
        _availabilityRepository = availabilityRepository;
        _notificationService = notificationService;
        _bookingOptions = bookingOptions.Value;
        _pricingOptions = pricingOptions.Value;
    }

    public async Task<BookingPreviewDto> PreviewAsync(int fieldId, DateOnly date, TimeOnly startTime, int durationHours, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        ValidateDuration(durationHours);

        var endTime = startTime.AddHours(durationHours);
        var availability = await _availabilityRepository.GetByFieldAndDateAsync(fieldId, date, ct);

        if (availability is not null && availability.IsClosed)
        {
            return Unavailable(field, date, startTime, endTime, durationHours, "Field is closed on the selected date.");
        }

        var openTime = availability?.OpenTime ?? new TimeOnly(8, 0);
        var closeTime = availability?.CloseTime ?? new TimeOnly(23, 0);

        if (startTime < openTime || endTime > closeTime)
        {
            return Unavailable(field, date, startTime, endTime, durationHours, "The selected time is outside operating hours.");
        }

        var hasConflict = await _bookingRepository.HasConflictingBookingAsync(fieldId, date, startTime, endTime, ct);
        if (hasConflict)
        {
            return Unavailable(field, date, startTime, endTime, durationHours, "The selected slot is no longer available.");
        }

        var priceLines = BuildPriceLines(field, startTime, endTime);
        var total = priceLines.Sum(p => p.LineTotal);

        return new BookingPreviewDto(
            field.Id,
            field.Name,
            date,
            startTime,
            endTime,
            durationHours,
            true,
            total,
            priceLines,
            "Slot is available.");
    }

    public async Task<BookingDto> CreateAsync(int userId, CreateBookingRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User was not found.");

        if (!user.IsActive)
        {
            throw new ForbiddenException("Inactive users cannot create bookings.");
        }

        var preview = await PreviewAsync(request.FieldId, request.Date, request.StartTime, request.DurationHours, ct);
        if (!preview.IsAvailable)
        {
            throw new ConflictException(preview.Message ?? "The selected slot is no longer available.");
        }

        var field = await _fieldRepository.GetByIdAsync(request.FieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        if (!field.IsActive || !field.IsApproved)
        {
            throw new ConflictException("This field is not available for booking.");
        }

        // Re-check for conflicts inside the transaction to prevent double bookings.
        await using var transaction = await _bookingRepository.BeginTransactionAsync(ct);
        var hasConflict = await _bookingRepository.HasConflictingBookingAsync(
            request.FieldId, request.Date, request.StartTime, preview.EndTime, ct);

        if (hasConflict)
        {
            throw new ConflictException("This time slot was booked by another user. Please choose another slot.");
        }

        var booking = new Booking
        {
            UserId = userId,
            FieldId = field.Id,
            BookingDate = request.Date,
            StartTime = request.StartTime,
            EndTime = preview.EndTime,
            DurationHours = request.DurationHours,
            TotalPrice = preview.TotalPrice,
            Status = BookingStatus.PendingPayment,
            ConcurrencyStamp = Guid.NewGuid().ToByteArray()
        };

        await _bookingRepository.AddAsync(booking, ct);
        await _bookingRepository.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await _notificationService.CreateAsync(
            userId,
            "Booking created",
            $"Your booking at \"{field.Name}\" on {request.Date} is awaiting payment.",
            NotificationType.BookingCreated, ct);

        return MapBooking(booking, field);
    }

    public async Task<IReadOnlyCollection<BookingDto>> GetUserBookingsAsync(int userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId, ct);
        var result = new List<BookingDto>();
        foreach (var booking in bookings)
        {
            result.Add(MapBooking(booking, booking.Field));
        }

        return result;
    }

    public async Task<IReadOnlyCollection<BookingDto>> GetUpcomingAsync(int userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return bookings
            .Where(b => b.BookingDate >= today && BookingStatusExtensions.OccupyingStatuses.Contains(b.Status))
            .Select(b => MapBooking(b, b.Field))
            .ToList();
    }

    public async Task<IReadOnlyCollection<BookingDto>> GetPastAsync(int userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return bookings
            .Where(b => b.BookingDate < today || b.Status is BookingStatus.Completed or BookingStatus.Cancelled or BookingStatus.Expired)
            .Select(b => MapBooking(b, b.Field))
            .ToList();
    }

    public async Task<BookingStatsDto> GetStatsAsync(int userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetUserBookingsAsync(userId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new BookingStatsDto(
            bookings.Count,
            bookings.Count(b => b.BookingDate >= today && BookingStatusExtensions.OccupyingStatuses.Contains(b.Status)),
            bookings.Count(b => b.BookingDate < today || b.Status is BookingStatus.Completed or BookingStatus.Cancelled or BookingStatus.Expired),
            bookings.Count(b => b.Status == BookingStatus.PendingPayment),
            bookings.Count(b => b.Status == BookingStatus.Confirmed),
            bookings.Count(b => b.Status == BookingStatus.Completed),
            bookings.Count(b => b.Status == BookingStatus.Cancelled),
            bookings.Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Completed).Sum(b => b.TotalPrice));
    }

    public async Task<BookingDto> GetByIdAsync(int userId, int bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to access this booking.");
        }

        return MapBooking(booking, booking.Field);
    }

    public async Task<BookingDto> CancelAsync(int userId, int bookingId, string? reason, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to cancel this booking.");
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
        {
            throw new ValidationDomainException("This booking cannot be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;
        booking.CancellationReason = reason?.Trim();
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync(ct);

        await _notificationService.CreateAsync(
            userId,
            "Booking cancelled",
            $"Your booking at \"{booking.Field.Name}\" on {booking.BookingDate} was cancelled.",
            NotificationType.BookingCancelled, ct);

        return MapBooking(booking, booking.Field);
    }

    private void ValidateDuration(int durationHours)
    {
        if (durationHours < _bookingOptions.MinDurationHours)
        {
            throw new ValidationDomainException($"Duration must be at least {_bookingOptions.MinDurationHours} hour(s).");
        }

        if (durationHours > _bookingOptions.MaxDurationHours)
        {
            throw new ValidationDomainException($"Duration cannot exceed {_bookingOptions.MaxDurationHours} hours.");
        }
    }

    private IReadOnlyCollection<PriceLineDto> BuildPriceLines(Field field, TimeOnly startTime, TimeOnly endTime)
    {
        var lines = new List<PriceLineDto>();
        var current = startTime;

        while (current < endTime)
        {
            var next = current.AddHours(1);
            var isNight = _pricingOptions.IsNight(current);
            var rate = isNight ? field.NightPricePerHour : field.DayPricePerHour;
            var period = isNight ? DayPeriod.Night : DayPeriod.Day;

            lines.Add(new PriceLineDto(current, next, period, rate, 1, rate));
            current = next;
        }

        return lines;
    }

    private static BookingPreviewDto Unavailable(Field field, DateOnly date, TimeOnly startTime, TimeOnly endTime, int durationHours, string message)
        => new(field.Id, field.Name, date, startTime, endTime, durationHours, false, 0m, Array.Empty<PriceLineDto>(), message);

    private static BookingDto MapBooking(Booking booking, Field field)
        => new(
            booking.Id,
            booking.FieldId,
            field.Name,
            field.City,
            booking.BookingDate,
            booking.StartTime,
            booking.EndTime,
            booking.DurationHours,
            booking.TotalPrice,
            booking.Status,
            booking.CreatedAtUtc);
}