using FluentAssertions;
using SportsBooking.Application.Common;
using Xunit;

namespace SportsBooking.Tests;

public sealed class GeoCalculatorTests
{
    [Theory]
    [InlineData(30.049, 31.240, 30.049, 31.240, 0)]
    [InlineData(30.0, 31.0, 30.0, 31.0, 0)]
    public void DistanceKm_Zero_ForIdenticalPoints(double lat1, double lon1, double lat2, double lon2, double expected)
    {
        var result = GeoCalculator.DistanceKm(lat1, lon1, lat2, lon2);
        result.Should().BeApproximately(expected, 0.01);
    }

    [Fact]
    public void DistanceKm_CairoToGiza_IsReasonable()
    {
        // Cairo (30.044, 31.236) to Giza (30.008, 31.210) ~ 4-5 km
        var result = GeoCalculator.DistanceKm(30.044, 31.236, 30.008, 31.210);
        result.Should().BeGreaterThan(3);
        result.Should().BeLessThan(6);
    }

    [Fact]
    public void IsWithinRadius_True_WhenClose()
    {
        GeoCalculator.IsWithinRadiusKm(30.0, 31.0, 30.0, 31.01, 2).Should().BeTrue();
    }

    [Fact]
    public void IsWithinRadius_False_WhenFar()
    {
        // ~111 km apart at the equator
        GeoCalculator.IsWithinRadiusKm(0, 0, 0, 1, 50).Should().BeFalse();
    }
}