using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Application.Services;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;
using Xunit;

namespace SportsBooking.Tests;

public sealed class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepo;
    private readonly Mock<IFieldRepository> _fieldRepo;
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IFieldAvailabilityRepository> _availabilityRepo;
    private readonly Mock<INotificationService> _notificationService;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _bookingRepo = new Mock<IBookingRepository>();
        _fieldRepo = new Mock<IFieldRepository>();
        _userRepo = new Mock<IUserRepository>();
        _availabilityRepo = new Mock<IFieldAvailabilityRepository>();
        _notificationService = new Mock<INotificationService>();

        var bookingOptions = Options.Create(new BookingOptions { MaxDurationHours = 4, MinDurationHours = 1 });
        var pricingOptions = Options.Create(new PricingOptions { DayStartHour = 8, NightStartHour = 18 });

        _service = new BookingService(
            _bookingRepo.Object,
            _fieldRepo.Object,
            _userRepo.Object,
            _availabilityRepo.Object,
            _notificationService.Object,
            bookingOptions,
            pricingOptions);
    }

    private static Field CreateField(decimal dayPrice = 100m, decimal nightPrice = 200m)
        => new()
        {
            Id = 1,
            Name = "Test Field",
            City = "Cairo",
            DayPricePerHour = dayPrice,
            NightPricePerHour = nightPrice,
            Sport = new Sport { Id = 1, Name = "Football", Type = SportType.Football }
        };

    [Fact]
    public async Task Preview_AvailableSlot_CalculatesDayPrice()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField());
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);
        _bookingRepo.Setup(r => r.HasConflictingBookingAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var result = await _service.PreviewAsync(1, date, new TimeOnly(10, 0), 2, CancellationToken.None);

        result.IsAvailable.Should().BeTrue();
        result.TotalPrice.Should().Be(200m); // 2 hours x 100 day rate
        result.PriceLines.Should().HaveCount(2);
        result.PriceLines.All(p => p.Period == DayPeriod.Day).Should().BeTrue();
    }

    [Fact]
    public async Task Preview_NightSlot_UsesNightPrice()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField(dayPrice: 100m, nightPrice: 200m));
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);
        _bookingRepo.Setup(r => r.HasConflictingBookingAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var result = await _service.PreviewAsync(1, date, new TimeOnly(20, 0), 1, CancellationToken.None);

        result.IsAvailable.Should().BeTrue();
        result.TotalPrice.Should().Be(200m);
        result.PriceLines.Single().Period.Should().Be(DayPeriod.Night);
    }

    [Fact]
    public async Task Preview_OverlappingBooking_ReturnsUnavailable()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField());
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);
        _bookingRepo.Setup(r => r.HasConflictingBookingAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var result = await _service.PreviewAsync(1, date, new TimeOnly(10, 0), 2, CancellationToken.None);

        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task Create_DurationExceedsMax_ThrowsValidation()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField());
        _userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, IsActive = true });
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);
        _bookingRepo.Setup(r => r.HasConflictingBookingAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var request = new CreateBookingRequest(1, date, new TimeOnly(10, 0), 5);

        var act = async () => await _service.CreateAsync(1, request, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationDomainException>();
    }

    [Fact]
    public async Task Create_ConflictDetected_ThrowsConflict()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField());
        _userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, IsActive = true });
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);

        // First call (preview) returns no conflict, second call (in transaction) returns conflict
        var sequence = new Queue<bool>(new[] { true });
        _bookingRepo.Setup(r => r.HasConflictingBookingAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var request = new CreateBookingRequest(1, date, new TimeOnly(10, 0), 1);

        var act = async () => await _service.CreateAsync(1, request, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesBooking()
    {
        var user = new User { Id = 1, IsActive = true };
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField());
        _userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);
        _bookingRepo.Setup(r => r.HasConflictingBookingAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _bookingRepo.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ITransaction>().Object);
        _bookingRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var request = new CreateBookingRequest(1, date, new TimeOnly(10, 0), 1);

        var result = await _service.CreateAsync(1, request, CancellationToken.None);

        result.Should().NotBeNull();
        result.FieldId.Should().Be(1);
        result.TotalPrice.Should().Be(100m);
        _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}