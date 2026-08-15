using FluentAssertions;
using Moq;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Services;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;
using Xunit;

namespace SportsBooking.Tests;

public sealed class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepo;
    private readonly Mock<IBookingRepository> _bookingRepo;
    private readonly Mock<IFieldRepository> _fieldRepo;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _reviewRepo = new Mock<IReviewRepository>();
        _bookingRepo = new Mock<IBookingRepository>();
        _fieldRepo = new Mock<IFieldRepository>();
        _service = new ReviewService(_reviewRepo.Object, _bookingRepo.Object, _fieldRepo.Object);
    }

    private static Booking CreateCompletedBooking(int userId = 1, int fieldId = 1)
    {
        var user = new User { Id = userId, FirstName = "John", LastName = "Doe" };
        return new Booking
        {
            Id = 1,
            UserId = userId,
            FieldId = fieldId,
            Status = BookingStatus.Completed,
            User = user
        };
    }

    [Fact]
    public async Task Create_ValidReview_AddsReviewAndUpdatesRating()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateCompletedBooking());
        _reviewRepo.Setup(r => r.GetByBookingIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Review?)null);
        _reviewRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _reviewRepo.Setup(r => r.GetFieldReviewsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Review> { new() { Rating = 4 } });
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Field { Id = 1 });

        var request = new CreateReviewRequest(1, 4, "Great field!");
        var result = await _service.CreateAsync(1, request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Rating.Should().Be(4);
        _reviewRepo.Verify(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_BookingNotCompleted_ThrowsValidation()
    {
        var booking = CreateCompletedBooking();
        booking.Status = BookingStatus.Confirmed;
        _bookingRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var request = new CreateReviewRequest(1, 5, null);
        var act = async () => await _service.CreateAsync(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>();
    }

    [Fact]
    public async Task Create_AnotherUsersBooking_ThrowsForbidden()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateCompletedBooking(userId: 2));

        var request = new CreateReviewRequest(1, 5, null);
        var act = async () => await _service.CreateAsync(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Create_DuplicateReview_ThrowsConflict()
    {
        _bookingRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateCompletedBooking());
        _reviewRepo.Setup(r => r.GetByBookingIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Review { Id = 9 });

        var request = new CreateReviewRequest(1, 5, null);
        var act = async () => await _service.CreateAsync(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Create_InvalidRating_ThrowsValidation(int rating)
    {
        var request = new CreateReviewRequest(1, rating, null);
        var act = async () => await _service.CreateAsync(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>();
    }
}