using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class AdminFieldService : IAdminFieldService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;

    public AdminFieldService(
        IFieldRepository fieldRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IAuditLogService auditLogService)
    {
        _fieldRepository = fieldRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<AdminFieldDto>> GetFieldsAsync(int page, int pageSize, bool? pendingOnly, CancellationToken ct = default)
    {
        var fields = await _fieldRepository.GetAllFieldsAsync(ct);

        var filtered = pendingOnly == true
            ? fields.Where(f => !f.IsApproved)
            : fields.AsEnumerable();

        var total = filtered.Count();
        var items = filtered
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((Math.Max(page, 1) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .Select(Map)
            .ToList();

        return new PagedResult<AdminFieldDto>(items, total, page, pageSize);
    }

    public async Task<AdminFieldDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetFieldDetailsAsync(id, ct)
            ?? throw new NotFoundException("Field was not found.");
        return Map(field);
    }

    public async Task<AdminFieldDto> SetApprovalAsync(int id, SetFieldApprovalRequest request, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetFieldDetailsAsync(id, ct)
            ?? throw new NotFoundException("Field was not found.");

        field.IsApproved = request.IsApproved;
        field.ApprovedAtUtc = request.IsApproved ? DateTime.UtcNow : null;
        field.UpdatedAtUtc = DateTime.UtcNow;

        await _fieldRepository.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(null, request.IsApproved ? "ApproveField" : "RejectField", nameof(Domain.Entities.Field), id.ToString(), request.Reason, ct);

        if (field.OwnerId.HasValue)
        {
            await _notificationService.CreateAsync(
                field.OwnerId.Value,
                request.IsApproved ? "Field approved" : "Field rejected",
                $"Your field \"{field.Name}\" was {(request.IsApproved ? "approved" : "rejected")}.",
                Domain.Enums.NotificationType.FieldApproved, ct);
        }

        return Map(field);
    }

    public async Task<AdminFieldDto> SetStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        var field = await _fieldRepository.GetFieldDetailsAsync(id, ct)
            ?? throw new NotFoundException("Field was not found.");

        field.IsActive = request.IsActive;
        field.UpdatedAtUtc = DateTime.UtcNow;

        await _fieldRepository.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(null, request.IsActive ? "ActivateField" : "DeactivateField", nameof(Domain.Entities.Field), id.ToString(), null, ct);
        return Map(field);
    }

    private static AdminFieldDto Map(Domain.Entities.Field f)
        => new(
            f.Id,
            f.Name,
            f.City,
            f.Sport.Type,
            f.Sport.Name,
            f.Owner is not null ? $"{f.Owner.FirstName} {f.Owner.LastName}".Trim() : null,
            f.DayPricePerHour,
            f.NightPricePerHour,
            f.AverageRating,
            f.IsActive,
            f.IsApproved,
            f.CreatedAtUtc);
}
