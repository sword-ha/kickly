using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Options;
using SportsBooking.Application.Services;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using Xunit;

namespace SportsBooking.Tests;

public sealed class FieldServiceTests
{
    private readonly Mock<IFieldRepository> _fieldRepo;
    private readonly Mock<IBookingRepository> _bookingRepo;
    private readonly Mock<IReviewRepository> _reviewRepo;
    private readonly Mock<IFieldAvailabilityRepository> _availabilityRepo;
    private readonly FieldService _service;

    public FieldServiceTests()
    {
        _fieldRepo = new Mock<IFieldRepository>();
        _bookingRepo = new Mock<IBookingRepository>();
        _reviewRepo = new Mock<IReviewRepository>();
        _availabilityRepo = new Mock<IFieldAvailabilityRepository>();

        var locationOptions = Options.Create(new LocationOptions { DefaultRadiusKm = 10, MaxRadiusKm = 100 });
        _service = new FieldService(_fieldRepo.Object, _bookingRepo.Object, _reviewRepo.Object, _availabilityRepo.Object, locationOptions);
    }

    private static Field CreateField(int id, string name, string city, decimal lat, decimal lon, decimal rating = 4.0m, int reviewCount = 10)
        => new()
        {
            Id = id,
            Name = name,
            City = city,
            Address = $"{city} address",
            Latitude = lat,
            Longitude = lon,
            DayPricePerHour = 100m,
            NightPricePerHour = 200m,
            AverageRating = rating,
            ReviewCount = reviewCount,
            IsActive = true,
            Sport = new Sport { Id = 1, Name = "Football", Type = SportType.Football },
            Images = new List<FieldImage>(),
            Amenities = new List<FieldAmenity>(),
            Bookings = new List<Booking>()
        };

    [Fact]
    public async Task GetNearby_ReturnsOnlyFieldsWithinRadius()
    {
        var fields = new List<Field>
        {
            CreateField(1, "Near Field", "Cairo", 30.049m, 31.240m),
            CreateField(2, "Far Field", "Alexandria", 31.200m, 29.950m)
        };
        _fieldRepo.Setup(r => r.GetAllFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(fields);

        var query = new NearbyFieldsQuery(30.049m, 31.240m, 5);
        var result = await _service.GetNearbyAsync(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetTopRated_FiltersByRadiusBeforeSorting()
    {
        var fields = new List<Field>
        {
            CreateField(1, "High Rated Far", "Alexandria", 31.200m, 29.950m, rating: 5.0m, reviewCount: 100),
            CreateField(2, "Medium Rated Near", "Cairo", 30.049m, 31.240m, rating: 4.0m, reviewCount: 50),
            CreateField(3, "Low Rated Near", "Cairo", 30.050m, 31.241m, rating: 3.0m, reviewCount: 10)
        };
        _fieldRepo.Setup(r => r.GetAllFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(fields);

        var result = await _service.GetTopRatedAsync(30.049, 31.240, 10, CancellationToken.None);

        // The 5-star field in Alexandria (~200km away) must be excluded by radius.
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(2); // 4.0 rating sorts before 3.0
        result.Last().Id.Should().Be(3);
    }

    [Fact]
    public async Task GetAvailability_ClosedDay_ReturnsClosed()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField(1, "Field", "Cairo", 30.0m, 31.0m));
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FieldAvailability { FieldId = 1, IsClosed = true });

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var result = await _service.GetAvailabilityAsync(1, date, CancellationToken.None);

        result.IsClosed.Should().BeTrue();
        result.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailability_OpenDay_BuildsSlotsConsideringBookings()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateField(1, "Field", "Cairo", 30.0m, 31.0m));
        _availabilityRepo.Setup(r => r.GetByFieldAndDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FieldAvailability?)null);
        _bookingRepo.Setup(r => r.GetFieldBookingsByDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Booking>
            {
                new() { FieldId = 1, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), Status = BookingStatus.Confirmed }
            });

        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var result = await _service.GetAvailabilityAsync(1, date, CancellationToken.None);

        result.IsClosed.Should().BeFalse();
        result.Slots.Should().NotBeEmpty();
        result.Slots.First(s => s.StartTime == new TimeOnly(10, 0)).IsAvailable.Should().BeFalse();
        result.Slots.First(s => s.StartTime == new TimeOnly(9, 0)).IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Search_FiltersByCityAndSortsByRating()
    {
        var fields = new List<Field>
        {
            CreateField(1, "Cairo Field A", "Cairo", 30.0m, 31.0m, rating: 3.0m),
            CreateField(2, "Cairo Field B", "Cairo", 30.0m, 31.0m, rating: 5.0m),
            CreateField(3, "Giza Field", "Giza", 30.0m, 31.0m, rating: 4.0m)
        };
        _fieldRepo.Setup(r => r.GetAllFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(fields);

        var query = new SearchFieldsQuery(City: "Cairo", SortBy: SortBy.Rating, Page: 1, PageSize: 10);
        var result = await _service.SearchAsync(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.First().Id.Should().Be(2); // 5.0 rating first
    }
}