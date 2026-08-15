using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Entities;

namespace SportsBooking.Application.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(int? userId, string action, string entityName, string entityId, string? details, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details
        };

        await _auditLogRepository.AddAsync(log, ct);
        await _auditLogRepository.SaveChangesAsync(ct);
    }
}
