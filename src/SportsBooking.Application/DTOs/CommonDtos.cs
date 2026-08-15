namespace SportsBooking.Application.DTOs;

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record ApiError(string Code, string Message, IReadOnlyCollection<string>? Details = null);