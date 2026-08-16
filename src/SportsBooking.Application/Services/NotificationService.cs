using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Enums;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }

    public async Task<NotificationSummaryDto> GetAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var skip = (page - 1) * pageSize;
        var items = await _notificationRepository.GetByUserIdAsync(userId, skip, pageSize, ct);
        var total = await _notificationRepository.CountByUserIdAsync(userId, ct);
        var unread = await _notificationRepository.CountUnreadAsync(userId, ct);

        return new NotificationSummaryDto(
            total,
            unread,
            items.Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAtUtc)).ToList());
    }

    public async Task<NotificationDto> MarkReadAsync(int userId, int notificationId, CancellationToken ct = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException("Notification was not found.");

        if (notification.UserId != userId)
        {
            throw new ForbiddenException("You can only update your own notifications.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            await _notificationRepository.SaveChangesAsync(ct);
        }

        return new NotificationDto(notification.Id, notification.Title, notification.Message, notification.Type, notification.IsRead, notification.CreatedAtUtc);
    }

    public async Task MarkAllReadAsync(int userId, CancellationToken ct = default)
    {
        var items = await _notificationRepository.GetByUserIdAsync(userId, 0, int.MaxValue, ct);
        var changed = false;

        foreach (var item in items.Where(n => !n.IsRead))
        {
            item.IsRead = true;
            item.ReadAtUtc = DateTime.UtcNow;
            changed = true;
        }

        if (changed)
        {
            await _notificationRepository.SaveChangesAsync(ct);
        }
    }

    public async Task CreateAsync(int userId, string title, string message, NotificationType type, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification, ct);
        await _notificationRepository.SaveChangesAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
        => await _notificationRepository.CountUnreadAsync(userId, ct);

    public async Task DeleteAsync(int userId, int notificationId, CancellationToken ct = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException("Notification was not found.");

        if (notification.UserId != userId)
        {
            throw new ForbiddenException("You can only delete your own notifications.");
        }

        _notificationRepository.Remove(notification);
        await _notificationRepository.SaveChangesAsync(ct);
    }

    public async Task<int> BroadcastAsync(string title, string message, CancellationToken ct = default)
    {
        var users = await _userRepository.GetPagedAsync(1, int.MaxValue, null, ct);
        var created = 0;

        foreach (var user in users.Where(u => u.IsActive))
        {
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = user.Id,
                Title = title,
                Message = message,
                Type = NotificationType.System,
                IsRead = false
            }, ct);
            created++;
        }

        if (created > 0)
        {
            await _notificationRepository.SaveChangesAsync(ct);
        }

        return created;
    }
}
