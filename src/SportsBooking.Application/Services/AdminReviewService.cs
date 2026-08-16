using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class AdminReviewService : IAdminReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IAuditLogService _auditLogService;

    public AdminReviewService(
        IReviewRepository reviewRepository,
        IFieldRepository fieldRepository,
        IAuditLogService auditLogService)
    {
        _reviewRepository = reviewRepository;
        _fieldRepository = fieldRepository;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<AdminReviewDto>> GetReviewsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _reviewRepository.GetPagedAsync(page, pageSize, ct);

        return new PagedResult<AdminReviewDto>(
            items.Select(Map).ToList(),
            total,
            page,
            pageSize);
    }

    public async Task<AdminReviewDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Review was not found.");

        return Map(review);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Review was not found.");

        var fieldId = review.FieldId;
        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            null, "Delete", nameof(Domain.Entities.Review), id.ToString(), null, ct);

        var reviews = await _reviewRepository.GetFieldReviewsAsync(fieldId, ct);
        var field = await _fieldRepository.GetByIdAsync(fieldId, ct);
        if (field is not null)
        {
            field.ReviewCount = reviews.Count;
            field.AverageRating = reviews.Count == 0 ? 0m : Math.Round((decimal)reviews.Average(r => r.Rating), 2);
            field.UpdatedAtUtc = DateTime.UtcNow;
            await _fieldRepository.SaveChangesAsync(ct);
        }
    }

    private static AdminReviewDto Map(Domain.Entities.Review r)
        => new(
            r.Id,
            r.BookingId,
            r.FieldId,
            r.Field.Name,
            r.UserId,
            $"{r.User.FirstName} {r.User.LastName}".Trim(),
            r.Rating,
            r.Comment,
            r.CreatedAtUtc);
}
