using FluentAssertions;
using SportsBooking.Application.DTOs;
using SportsBooking.Application.Validators;
using Xunit;

namespace SportsBooking.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void RegisterValidator_ValidRequest_Passes()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("John", "Doe", "john@test.com", "01000000000", "password123");
        var result = validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterValidator_InvalidEmail_Fails()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("John", "Doe", "not-an-email", "01000000000", "password123");
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RegisterValidator_ShortPassword_Fails()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("John", "Doe", "john@test.com", "01000000000", "123");
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateBookingValidator_PastDate_Fails()
    {
        var validator = new CreateBookingRequestValidator();
        var request = new CreateBookingRequest(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(10, 0), 2);
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateBookingValidator_ValidRequest_Passes()
    {
        var validator = new CreateBookingRequestValidator();
        var request = new CreateBookingRequest(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(10, 0), 2);
        var result = validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateReviewValidator_ValidRating_Passes()
    {
        var validator = new CreateReviewRequestValidator();
        var request = new CreateReviewRequest(1, 4, "Good");
        var result = validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CreateReviewValidator_OutOfRangeRating_Fails(int rating)
    {
        var validator = new CreateReviewRequestValidator();
        var request = new CreateReviewRequest(1, rating, "Good");
        var result = validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}