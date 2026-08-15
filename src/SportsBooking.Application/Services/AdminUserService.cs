using SportsBooking.Application.DTOs;
using SportsBooking.Application.Interfaces;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.Application.Services;

public sealed class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;

    public AdminUserService(IUserRepository userRepository, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var users = await _userRepository.GetPagedAsync(page, pageSize, search, ct);
        var total = await _userRepository.CountAsync(ct);
        return new PagedResult<AdminUserDto>(users.Select(Map).ToList(), total, page, pageSize);
    }

    public async Task<AdminUserDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User was not found.");
        return Map(user);
    }

    public async Task<AdminUserDto> SetStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User was not found.");

        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(id, request.IsActive ? "Activate" : "Deactivate", nameof(Domain.Entities.User), id.ToString(), null, ct);
        return Map(user);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User was not found.");

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync(ct);
        await _auditLogService.LogAsync(id, "Delete", nameof(Domain.Entities.User), id.ToString(), null, ct);
    }

    private static AdminUserDto Map(Domain.Entities.User u)
        => new(u.Id, u.FirstName, u.LastName, u.Email ?? string.Empty, u.PhoneNumber ?? string.Empty, u.Role, u.IsActive, u.CreatedAtUtc);
}
