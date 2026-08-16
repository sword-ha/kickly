using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;

namespace SportsBooking.Application.Services;

public sealed class AdminAuditLogService : IAdminAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminAuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _auditLogRepository.GetPagedAsync(page, pageSize, ct);

        return new PagedResult<AuditLogDto>(
            items.Select(l => new AuditLogDto(
                    l.Id,
                    l.UserId,
                    l.Action,
                    l.EntityName,
                    l.EntityId,
                    l.Details,
                    l.CreatedAtUtc))
                .ToList(),
            total,
            page,
            pageSize);
    }
}
