using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldRepository _fieldRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IBookingRepository bookingRepository,
        IFieldRepository fieldRepository)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _fieldRepository = fieldRepository;
    }

    public async Task<ReviewDto> CreateAsync(int userId, CreateReviewRequest request, CancellationToken ct = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ValidationDomainException("Rating must be between 1 and 5.");
        }

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, ct)
            ?? throw new NotFoundException("Booking was not found.");

        if (booking.UserId != userId)
        {
            throw new ForbiddenException("You can only review your own bookings.");
        }

        if (booking.Status != BookingStatus.Completed)
        {
            throw new ValidationDomainException("Only completed bookings can be reviewed.");
        }

        if (await _reviewRepository.GetByBookingIdAsync(request.BookingId, ct) is not null)
        {
            throw new ConflictException("A review already exists for this booking.");
        }

        var review = new Review
        {
            BookingId = booking.Id,
            UserId = userId,
            FieldId = booking.FieldId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim()
        };

        await _reviewRepository.AddAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);

        await RecalculateRatingAsync(booking.FieldId, ct);

        var field = await _fieldRepository.GetByIdAsync(booking.FieldId, ct);
        return new ReviewDto(
            review.Id,
            review.BookingId,
            review.UserId,
            $"{booking.User.FirstName} {booking.User.LastName}".Trim(),
            review.FieldId,
            review.Rating,
            review.Comment,
            review.CreatedAtUtc);
    }

    public async Task<ReviewDto> UpdateAsync(int userId, int reviewId, UpdateReviewRequest request, CancellationToken ct = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ValidationDomainException("Rating must be between 1 and 5.");
        }

        var review = await _reviewRepository.GetByIdAsync(reviewId, ct)
            ?? throw new NotFoundException("Review was not found.");

        if (review.UserId != userId)
        {
            throw new ForbiddenException("You can only update your own reviews.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment?.Trim();
        review.UpdatedAtUtc = DateTime.UtcNow;

        await _reviewRepository.SaveChangesAsync(ct);
        await RecalculateRatingAsync(review.FieldId, ct);

        return new ReviewDto(
            review.Id,
            review.BookingId,
            review.UserId,
            string.Empty,
            review.FieldId,
            review.Rating,
            review.Comment,
            review.CreatedAtUtc);
    }

    public async Task DeleteAsync(int userId, int reviewId, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, ct)
            ?? throw new NotFoundException("Review was not found.");

        if (review.UserId != userId)
        {
            throw new ForbiddenException("You can only delete your own reviews.");
        }

        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync(ct);
        await RecalculateRatingAsync(review.FieldId, ct);
    }

    private async Task RecalculateRatingAsync(int fieldId, CancellationToken ct)
    {
        var reviews = await _reviewRepository.GetFieldReviewsAsync(fieldId, ct);
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct)
            ?? throw new NotFoundException("Field was not found.");

        field.ReviewCount = reviews.Count;
        field.AverageRating = reviews.Count == 0 ? 0m : Math.Round((decimal)reviews.Average(r => r.Rating), 2);
        field.UpdatedAtUtc = DateTime.UtcNow;

        await _fieldRepository.SaveChangesAsync(ct);
    }
}