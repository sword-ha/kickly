using SportsBooking.Domain.Enums;

namespace SportsBooking.Application.DTOs;

public sealed record NotificationDto(int Id, string Title, string Message, NotificationType Type, bool IsRead, DateTime CreatedAtUtc);

public sealed record NotificationSummaryDto(int TotalCount, int UnreadCount, IReadOnlyCollection<NotificationDto> Items);
